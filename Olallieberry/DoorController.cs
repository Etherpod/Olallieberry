using UnityEngine;

namespace Olallieberry;

public class DoorController : MonoBehaviour
{
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

    protected Vector3 _closedLocalPosition;
    protected bool _isOpen;

    public bool IsOpen => _isOpen;

    protected virtual void Awake()
    {
        if (movingPart == null)
        {
            movingPart = transform;
        }

        _closedLocalPosition = movingPart.localPosition;

        if (audioSource == null)
        {
            audioSource = GetComponentInChildren<OWAudioSource>();
        }
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

    public virtual void Open()
    {
        SetOpen(true);
    }

    public virtual void Close()
    {
        SetOpen(false);
    }

    public virtual void SetOpen(bool open)
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

    public virtual void Toggle()
    {
        SetOpen(!_isOpen);
    }
}