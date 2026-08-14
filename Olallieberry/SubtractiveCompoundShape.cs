using System;
using System.Collections.Generic;
using UnityEngine;

namespace Olallieberry;

[AddComponentMenu("Shapes/Subtractive Compound Shape", 101)]
public class SubtractiveCompoundShape : Shape
{
    [Header("Shape Roots")]
    [Tooltip("All Shape components under this transform count as positive.")]
    public Transform positiveRoot;

    [Tooltip("All Shape components under this transform count as negative and subtract from the positive shapes.")]
    public Transform negativeRoot;

    private Shape[] _positiveShapes = Array.Empty<Shape>();
    private Shape[] _negativeShapes = Array.Empty<Shape>();

    private readonly Dictionary<Shape, CollisionState> _collisions =
        new Dictionary<Shape, CollisionState>(32);

    private readonly List<Shape> _enterBuffer = new List<Shape>(8);
    private readonly List<Shape> _exitBuffer = new List<Shape>(8);
    private readonly List<Shape> _removeBuffer = new List<Shape>(8);

    private bool _collisionStateDirty;

    public override int layerMask
    {
        get => base.layerMask;
        set
        {
            base.layerMask = value;

            SetLayerMask(_positiveShapes, value);
            SetLayerMask(_negativeShapes, value);
        }
    }

    public override bool pointChecksOnly
    {
        get => base.pointChecksOnly;
        set
        {
            base.pointChecksOnly = value;

            SetPointChecksOnly(_positiveShapes, value);
            SetPointChecksOnly(_negativeShapes, value);
        }
    }

    public override void Awake()
    {
        base.Awake();

        _positiveShapes = CollectShapes(positiveRoot);
        _negativeShapes = CollectShapes(negativeRoot);

        InitializeShapes(_positiveShapes, false);
        InitializeShapes(_negativeShapes, true);
    }

    public virtual void Start()
    {
        RecalculateLocalBounds();
    }

    public void Update()
    {
        FlushCollisionState(false);
    }

    public virtual void OnDestroy()
    {
        UnsubscribeShapes(_positiveShapes, false);
        UnsubscribeShapes(_negativeShapes, true);
    }

    public override void OnEnable()
    {
        SetEnabled(_positiveShapes, true);
        SetEnabled(_negativeShapes, true);
    }

    public override void OnDisable()
    {
        SetEnabled(_positiveShapes, false);
        SetEnabled(_negativeShapes, false);

        // Update() won't run while disabled, so flush exits now.
        FlushCollisionState(true);
    }

    public override void RecalculateLocalBounds()
    {
        if (_positiveShapes == null || _positiveShapes.Length == 0)
        {
            return;
        }

        bool initialized = false;

        foreach (Shape shape in _positiveShapes)
        {
            if (shape == null)
            {
                continue;
            }

            if (!initialized)
            {
                _localBounds = shape.localBounds;
                initialized = true;
            }
            else
            {
                _localBounds.Encapsulate(shape.localBounds);
            }
        }
    }

    public override Vector3 GetWorldSpaceCenter()
    {
        if (_positiveShapes == null || _positiveShapes.Length == 0)
        {
            return transform.position;
        }

        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (Shape shape in _positiveShapes)
        {
            if (shape == null)
            {
                continue;
            }

            center += shape.GetWorldSpaceCenter();
            count++;
        }

        return count > 0 ? center / count : transform.position;
    }

    public override bool PointInside(Vector3 point)
    {
        bool insidePositive = false;

        foreach (Shape shape in _positiveShapes)
        {
            if (shape != null && shape.PointInside(point))
            {
                insidePositive = true;
                break;
            }
        }

        if (!insidePositive)
        {
            return false;
        }

        foreach (Shape shape in _negativeShapes)
        {
            if (shape != null && shape.PointInside(point))
            {
                return false;
            }
        }

        return true;
    }

    public override void SetCollisionMode(CollisionMode newCollisionMode)
    {
        _collisionMode = newCollisionMode;

        SetCollisionMode(_positiveShapes, newCollisionMode);
        SetCollisionMode(_negativeShapes, newCollisionMode);
    }

    public override void SetLayer(Layer newLayer)
    {
        _layer = newLayer;

        SetLayer(_positiveShapes, newLayer);
        SetLayer(_negativeShapes, newLayer);
    }

    public override void SetActivation(bool newActive)
    {
        if (_active == newActive)
        {
            return;
        }

        _active = newActive;

        SetActivation(_positiveShapes, newActive);
        SetActivation(_negativeShapes, newActive);

        if (!newActive)
        {
            FlushCollisionState(true);
        }
    }

    private Shape[] CollectShapes(Transform root)
    {
        if (root == null)
        {
            return Array.Empty<Shape>();
        }

        Shape[] found = root.GetComponentsInChildren<Shape>(true);
        List<Shape> shapes = new List<Shape>(found.Length);

        foreach (Shape shape in found)
        {
            if (shape == null || shape == this)
            {
                continue;
            }

            shapes.Add(shape);
        }

        return shapes.ToArray();
    }

    private void InitializeShapes(Shape[] shapes, bool negative)
    {
        foreach (Shape shape in shapes)
        {
            if (shape == null)
            {
                continue;
            }

            shape.SetCollisionMode(_collisionMode);
            shape.SetLayer(_layer);
            shape.layerMask = _layerMask;
            shape.pointChecksOnly = _pointChecksOnly;

            if (negative)
            {
                shape.OnCollisionEnter += OnNegativeCollisionEnter;
                shape.OnCollisionExit += OnNegativeCollisionExit;
            }
            else
            {
                shape.OnCollisionEnter += OnPositiveCollisionEnter;
                shape.OnCollisionExit += OnPositiveCollisionExit;
            }
        }
    }

    private void UnsubscribeShapes(Shape[] shapes, bool negative)
    {
        foreach (Shape shape in shapes)
        {
            if (shape == null)
            {
                continue;
            }

            if (negative)
            {
                shape.OnCollisionEnter -= OnNegativeCollisionEnter;
                shape.OnCollisionExit -= OnNegativeCollisionExit;
            }
            else
            {
                shape.OnCollisionEnter -= OnPositiveCollisionEnter;
                shape.OnCollisionExit -= OnPositiveCollisionExit;
            }
        }
    }

    private CollisionState GetCollisionState(Shape otherShape)
    {
        if (!_collisions.TryGetValue(otherShape, out CollisionState state))
        {
            state = new CollisionState();
            _collisions.Add(otherShape, state);
        }

        return state;
    }

    private void OnPositiveCollisionEnter(Shape otherShape)
    {
        CollisionState state = GetCollisionState(otherShape);
        state.positiveCount++;
        _collisionStateDirty = true;
    }

    private void OnPositiveCollisionExit(Shape otherShape)
    {
        if (_collisions.TryGetValue(otherShape, out CollisionState state))
        {
            state.positiveCount = Mathf.Max(0, state.positiveCount - 1);
            _collisionStateDirty = true;
        }
    }

    private void OnNegativeCollisionEnter(Shape otherShape)
    {
        CollisionState state = GetCollisionState(otherShape);
        state.negativeCount++;
        _collisionStateDirty = true;
    }

    private void OnNegativeCollisionExit(Shape otherShape)
    {
        if (_collisions.TryGetValue(otherShape, out CollisionState state))
        {
            state.negativeCount = Mathf.Max(0, state.negativeCount - 1);
            _collisionStateDirty = true;
        }
    }

    private void FlushCollisionState(bool forceExit)
    {
        if (!_collisionStateDirty && !forceExit)
        {
            return;
        }

        _collisionStateDirty = false;

        _enterBuffer.Clear();
        _exitBuffer.Clear();
        _removeBuffer.Clear();

        foreach (KeyValuePair<Shape, CollisionState> pair in _collisions)
        {
            Shape otherShape = pair.Key;
            CollisionState state = pair.Value;

            bool shouldBeColliding =
                !forceExit &&
                state.positiveCount > 0 &&
                state.negativeCount == 0;

            if (state.colliding != shouldBeColliding)
            {
                state.colliding = shouldBeColliding;

                if (shouldBeColliding)
                {
                    _enterBuffer.Add(otherShape);
                }
                else
                {
                    _exitBuffer.Add(otherShape);
                }
            }

            if (forceExit ||
                (state.positiveCount == 0 && state.negativeCount == 0))
            {
                _removeBuffer.Add(otherShape);
            }
        }

        for (int i = 0; i < _removeBuffer.Count; i++)
        {
            _collisions.Remove(_removeBuffer[i]);
        }

        // Fire after iterating the dictionary, since event listeners may
        // cause more shapes to be enabled/disabled.
        for (int i = 0; i < _exitBuffer.Count; i++)
        {
            FireCollisionExitEvent(_exitBuffer[i]);
        }

        for (int i = 0; i < _enterBuffer.Count; i++)
        {
            FireCollisionEnterEvent(_enterBuffer[i]);
        }

        if (forceExit)
        {
            _collisions.Clear();
        }
    }

    private static void SetEnabled(Shape[] shapes, bool enabled)
    {
        foreach (Shape shape in shapes)
        {
            if (shape == null)
                continue;

            shape.enabled = enabled;
        }
    }

    private static void SetLayerMask(Shape[] shapes, int layerMask)
    {
        foreach (Shape shape in shapes)
        {
            if (shape == null)
                continue;

            shape.layerMask = layerMask;
        }
    }

    private static void SetPointChecksOnly(Shape[] shapes, bool value)
    {
        foreach (Shape shape in shapes)
        {
            if (shape == null)
                continue;

            shape.pointChecksOnly = value;
        }
    }

    private static void SetCollisionMode(
        Shape[] shapes,
        CollisionMode collisionMode)
    {
        foreach (Shape shape in shapes)
        {
            if (shape == null)
                continue;

            shape.SetCollisionMode(collisionMode);
        }
    }

    private static void SetLayer(Shape[] shapes, Layer layer)
    {
        foreach (Shape shape in shapes)
        {
            if (shape == null)
                continue;

            shape.SetLayer(layer);
        }
    }

    private static void SetActivation(Shape[] shapes, bool active)
    {
        foreach (Shape shape in shapes)
        {
            if (shape == null)
                continue;

            shape.SetActivation(active);
        }
    }

    private class CollisionState
    {
        public int positiveCount;
        public int negativeCount;
        public bool colliding;
    }
}