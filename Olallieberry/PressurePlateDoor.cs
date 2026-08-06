using System.Collections.Generic;
using UnityEngine;

namespace Olallieberry;

/// <summary>
/// A sliding door controlled by one or more
/// <see cref="PressurePlate"/> objects.
/// </summary>
public class PressurePlateDoor : PressurePlateReceiver
{
    public enum ActivationMode
    {
        /// <summary>
        /// Opens when any controlling pressure plate is pressed.
        /// </summary>
        Any,

        /// <summary>
        /// Opens only while every controlling pressure plate is pressed.
        /// </summary>
        All
    }

    /// <summary>
    /// Determines whether any or all pressure plates are required to open the door.
    /// </summary>
    [Header("Activation")]
    [Tooltip("Determines whether any or all pressure plates are required to open the door.")]
    public ActivationMode activationMode = ActivationMode.Any;

    /// <summary>
    /// The number of pressure plates that control this door.
    /// Required when using the <see cref="ActivationMode.All"/> activation mode.
    /// </summary>
    [Tooltip("The number of pressure plates that control this door. Required when using the All activation mode.")]
    [Min(1)]
    public int requiredPlateCount = 1;

    /// <summary>
    /// The transform that moves when the door opens.
    /// </summary>
    [Header("Movement")]
    [Tooltip("The transform that moves when the door opens.")]
    public Transform movingPart;

    /// <summary>
    /// The door's open position relative to its initial local position.
    /// </summary>
    [Tooltip("The door's open position relative to its initial local position.")]
    public Vector3 openOffset = new(0f, 3f, 0f);

    /// <summary>
    /// The door's movement speed in units per second.
    /// </summary>
    [Tooltip("The door's movement speed in units per second.")]
    [Min(0f)]
    public float moveSpeed = 3f;

    /// <summary>
    /// The audio source used to play door sounds.
    /// </summary>
    [Header("Audio")]
    [Tooltip("The audio source used to play door sounds.")]
    public OWAudioSource audioSource;

    /// <summary>
    /// The sound played when the door begins opening.
    /// </summary>
    [Tooltip("The sound played when the door begins opening.")]
    public AudioType openSound = AudioType.NomaiDoorStart;

    /// <summary>
    /// The sound played when the door begins closing.
    /// </summary>
    [Tooltip("The sound played when the door begins closing.")]
    public AudioType closeSound = AudioType.NomaiDoorStop;

    private readonly HashSet<PressurePlate> _activePlates = [];

    private Vector3 _closedLocalPosition;
    private bool _isOpen;

    /// <summary>
    /// Whether the door is currently commanded to open.
    /// </summary>
    public bool IsOpen => _isOpen;

    protected virtual void Awake()
    {
        if (movingPart == null) movingPart = transform;
        if (audioSource == null) audioSource = GetComponentInChildren<OWAudioSource>();

        _closedLocalPosition = movingPart.localPosition;
    }

    protected virtual void Update()
    {
        Vector3 targetPosition = _isOpen
            ? _closedLocalPosition + openOffset
            : _closedLocalPosition;

        movingPart.localPosition = Vector3.MoveTowards(
            movingPart.localPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    /// <inheritdoc/>
    public override void SetPressurePlateState(
        PressurePlate pressurePlate,
        bool isPressed
    )
    {
        if (pressurePlate == null)
        {
            return;
        }

        if (isPressed)
        {
            _activePlates.Add(pressurePlate);
        }
        else
        {
            _activePlates.Remove(pressurePlate);
        }

        bool shouldOpen = activationMode switch
        {
            ActivationMode.Any => _activePlates.Count > 0,
            ActivationMode.All => _activePlates.Count >= requiredPlateCount,
            _ => false
        };

        SetOpen(shouldOpen);
    }

    public void SetOpen(bool open)
    {
        if (_isOpen == open)
        {
            return;
        }

        _isOpen = open;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(open ? openSound : closeSound);
        }
    }
}