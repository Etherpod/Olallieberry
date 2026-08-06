using Olallieberry.TimeZones;
using System.Collections.Generic;
using UnityEngine;

namespace Olallieberry;

/// <summary>
/// A Portal-style pressure plate activated by rigidbodies inside its trigger volume.
/// The scout does not activate the plate.
/// </summary>
[RequireComponent(typeof(OWTriggerVolume))]
public class PressurePlate : MonoBehaviour
{
    /// <summary>
    /// The trigger volume used to detect rigidbodies resting on the pressure plate.
    /// </summary>
    [Header("Detection")]
    [Tooltip("The trigger volume used to detect rigidbodies resting on the pressure plate.")]
    public OWTriggerVolume triggerVolume;

    /// <summary>
    /// Objects that receive the pressure plate's active state.
    /// </summary>
    [Header("Receivers")]
    [Tooltip("Objects that receive the pressure plate's active state.")]
    public PressurePlateReceiver[] receivers = [];

    /// <summary>
    /// The visual portion of the pressure plate that moves when pressed.
    /// </summary>
    [Header("Visuals")]
    [Tooltip("The visual portion of the pressure plate that moves when pressed.")]
    public Transform movingPart = null;

    /// <summary>
    /// The local displacement applied to the moving part when the plate is pressed.
    /// </summary>
    [Tooltip("The local displacement applied to the moving part when the plate is pressed.")]
    public Vector3 pressedOffset = new(0f, -0.05f, 0f);

    /// <summary>
    /// How quickly the moving part transitions between its pressed and released positions.
    /// </summary>
    [Tooltip("How quickly the moving part transitions between its pressed and released positions.")]
    [Min(0f)]
    public float visualMoveSpeed = 0.25f;

    /// <summary>
    /// The renderer whose emission color indicates whether the plate is active.
    /// </summary>
    [Tooltip("The renderer whose emission color indicates whether the plate is active.")]
    public MeshRenderer glowRenderer = null;
    
    /// <summary>
    /// The emission color used while the pressure plate is inactive.
    /// </summary>
    [Tooltip("The emission color used while the pressure plate is inactive.")]
    [ColorUsage(false, true)]
    public Color inactiveColor = Color.red;

    /// <summary>
    /// The emission color used while the pressure plate is active.
    /// </summary>
    [Tooltip("The emission color used while the pressure plate is active.")]
    [ColorUsage(false, true)]
    public Color activeColor = Color.green;

    /// <summary>
    /// How quickly the pressure plate's glow transitions between states.
    /// </summary>
    [Tooltip("How quickly the pressure plate's glow transitions between states.")]
    [Min(0f)]
    public float glowSpeed = 3f;

    /// <summary>
    /// The audio source used to play pressure plate sounds.
    /// </summary>
    [Header("Audio")]
    [Tooltip("The audio source used to play pressure plate sounds.")]
    public OWAudioSource audioSource;

    /// <summary>
    /// The sound played when the pressure plate is activated.
    /// </summary>
    [Tooltip("The sound played when the pressure plate is activated.")]
    public AudioType activationSound = AudioType.NomaiOrbSlotActivated;

    /// <summary>
    /// The sound played when the pressure plate is released.
    /// </summary>
    [Tooltip("The sound played when the pressure plate is released.")]
    public AudioType releaseSound = AudioType.NomaiOrbStartDrag;

    private readonly Dictionary<OWRigidbody, int> _occupants = [];

    private MaterialPropertyBlock _materialProperties;
    private Vector3 _releasedLocalPosition;
    private float _visualFraction;
    private bool _isPressed;

    /// <summary>
    /// Whether the pressure plate is currently pressed and its time zone is active.
    /// </summary>
    public bool IsPressed => _isPressed;

    protected virtual void Awake()
    {
        if (triggerVolume == null) triggerVolume = GetComponentInChildren<OWTriggerVolume>();
        if (audioSource == null) audioSource = GetComponentInChildren<OWAudioSource>();

        if (movingPart != null)
        {
            _releasedLocalPosition = movingPart.localPosition;
        }

        if (glowRenderer != null)
        {
            _materialProperties = new MaterialPropertyBlock();
        }
    }

    protected virtual void Start()
    {
        triggerVolume.OnEntry += OnTriggerEntry;
        triggerVolume.OnExit += OnTriggerExit;
    }

    protected virtual void OnDestroy()
    {
        triggerVolume.OnEntry -= OnTriggerEntry;
        triggerVolume.OnExit -= OnTriggerExit;
    }

    protected virtual void OnEnable()
    {
        RefreshState(false);
    }

    protected virtual void OnDisable()
    {
        SetPressed(false, false);
        _occupants.Clear();
    }

    protected virtual void Update()
    {
        UpdateVisuals();
    }

    private void OnTriggerEntry(GameObject hitObject)
    {
        if (!TryGetActivator(hitObject, out var rigidbody))
        {
            return;
        }

        _occupants.TryGetValue(rigidbody, out int colliderCount);
        _occupants[rigidbody] = colliderCount + 1;

        RefreshState(true);
    }

    private void OnTriggerExit(GameObject hitObject)
    {
        if (!TryGetActivator(hitObject, out var rigidbody))
        {
            return;
        }

        if (!_occupants.TryGetValue(rigidbody, out int colliderCount))
        {
            return;
        }

        if (colliderCount <= 1)
        {
            _occupants.Remove(rigidbody);
        }
        else
        {
            _occupants[rigidbody] = colliderCount - 1;
        }

        RefreshState(true);
    }

    private bool TryGetActivator(GameObject hitObject, out OWRigidbody rigidbody)
    {
        rigidbody = hitObject.GetComponentInParent<OWRigidbody>();

        if (rigidbody == null)
        {
            return false;
        }

        // The scout should not activate pressure plates.
        if (hitObject.CompareTag("ProbeDetector"))
        {
            return false;
        }

        return true;
    }

    private void RefreshState(bool playSound)
    {
        RemoveDestroyedOccupants();

        bool shouldBePressed = _occupants.Count > 0;

        SetPressed(shouldBePressed, playSound);
    }

    private void RemoveDestroyedOccupants()
    {
        if (_occupants.Count == 0)
        {
            return;
        }

        List<OWRigidbody> removedBodies = null;

        foreach (var occupant in _occupants)
        {
            if (occupant.Key == null)
            {
                removedBodies ??= [];
                removedBodies.Add(occupant.Key);
            }
        }

        if (removedBodies == null)
        {
            return;
        }

        foreach (var body in removedBodies)
        {
            _occupants.Remove(body);
        }
    }

    private void SetPressed(bool pressed, bool playSound)
    {
        if (_isPressed == pressed)
        {
            return;
        }

        _isPressed = pressed;

        foreach (var receiver in receivers)
        {
            if (receiver != null)
            {
                receiver.SetPressurePlateState(this, pressed);
            }
        }

        if (playSound && audioSource != null)
        {
            audioSource.PlayOneShot(
                pressed ? activationSound : releaseSound
            );
        }
    }

    private void UpdateVisuals()
    {
        float targetFraction = _isPressed ? 1f : 0f;

        _visualFraction = Mathf.MoveTowards(
            _visualFraction,
            targetFraction,
            glowSpeed * Time.deltaTime
        );

        if (movingPart != null)
        {
            Vector3 targetPosition =
                _releasedLocalPosition + pressedOffset * _visualFraction;

            movingPart.localPosition = Vector3.MoveTowards(
                movingPart.localPosition,
                targetPosition,
                visualMoveSpeed * Time.deltaTime
            );
        }

        if (glowRenderer != null)
        {
            Color emissionColor = Color.Lerp(
                inactiveColor,
                activeColor,
                _visualFraction
            );

            glowRenderer.GetPropertyBlock(_materialProperties);
            _materialProperties.SetColor("_EmissionColor", emissionColor);
            glowRenderer.SetPropertyBlock(_materialProperties);
        }
    }
}