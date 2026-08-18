using UnityEngine;

namespace Olallieberry;

public class SingleLeverController : AbstractLeverController
{
    /// <summary>
    /// The interact receiver used to operate the lever.
    /// </summary>
    [Header("Interaction")]
    [Tooltip("The interact receiver used to operate the lever.")]
    public InteractReceiver interactReceiver;

    /// <summary>
    /// The handle that rotates when the lever is toggled.
    /// </summary>
    [Header("Lever")]
    [Tooltip("The handle that rotates when the lever is toggled.")]
    public Transform handle;

    /// <summary>
    /// The number of degrees the handle rotates around its local X axis.
    /// </summary>
    [Tooltip("The number of degrees the handle rotates around its local X axis.")]
    public float rotationDegrees = 45f;

    /// <summary>
    /// The handle's rotation speed in degrees per second.
    /// </summary>
    [Tooltip("The handle's rotation speed in degrees per second.")]
    [Min(0f)]
    public float rotationSpeed = 60f;

    private Quaternion _inactiveRotation;
    private Quaternion _activeRotation;

    public void Awake()
    {
        _inactiveRotation = handle.localRotation;
        _activeRotation =
            _inactiveRotation * Quaternion.Euler(rotationDegrees, 0f, 0f);
    }

    public void Start()
    {
        if (interactReceiver != null)
        {
            interactReceiver.OnPressInteract += OnPressInteract;
            interactReceiver.SetPromptText("LEVER_PROMPT");
        }
    }

    public void OnDestroy()
    {
        if (interactReceiver != null)
            interactReceiver.OnPressInteract -= OnPressInteract;
    }

    public void Update()
    {
        Quaternion targetRotation =
            IsActive ? _activeRotation : _inactiveRotation;

        handle.localRotation = Quaternion.RotateTowards(
            handle.localRotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void OnPressInteract()
    {
        interactReceiver.ResetInteraction();
        Toggle();
    }
}