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
    /// The audio source used to play the door's looping movement sound.
    /// </summary>
    [Header("Audio")]
    [Tooltip("The audio source used to play the door's looping movement sound.")]
    public OWAudioSource loopingAudioSource;

    /// <summary>
    /// The audio source used to play the door's start and stop sounds.
    /// </summary>
    [Tooltip("The audio source used to play the door's start and stop sounds.")]
    public OWAudioSource oneShotAudioSource;

    /// <summary>
    /// The sound played when the door begins moving.
    /// </summary>
    private static readonly AudioType _startSound = AudioType.SecretPassage_Start;

    /// <summary>
    /// The sound played while the door is moving.
    /// </summary>
    private static readonly AudioType _loopSound = AudioType.SecretPassage_Loop;

    /// <summary>
    /// The sound played when the door finishes moving.
    /// </summary>
    private static readonly AudioType _stopSound = AudioType.SecretPassage_Stop;

    protected Vector3 _closedLocalPosition;
    protected bool _isOpen;
    protected bool _isMoving;

    public bool IsOpen => _isOpen;

    protected virtual void Awake()
    {
        if (movingPart == null)
        {
            movingPart = transform;
        }

        _closedLocalPosition = movingPart.localPosition;

        if (loopingAudioSource != null)
        {
            loopingAudioSource.AssignAudioLibraryClip(_loopSound);
        }
    }

    protected virtual void Update()
    {
        if (!_isMoving)
        {
            return;
        }

        Vector3 targetPosition = _isOpen
            ? _closedLocalPosition + openOffset
            : _closedLocalPosition;

        movingPart.localPosition = Vector3.MoveTowards(
            movingPart.localPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(movingPart.localPosition, targetPosition) < 0.001f)
        {
            movingPart.localPosition = targetPosition;
            _isMoving = false;

            if (oneShotAudioSource != null && loopingAudioSource != null)
            {
                oneShotAudioSource.PlayOneShot(_stopSound, 1f);
                loopingAudioSource.FadeOut(
                    0.2f,
                    OWAudioSource.FadeOutCompleteAction.STOP,
                    0f
                );
            }
        }
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
        _isMoving = true;

        if (oneShotAudioSource != null && loopingAudioSource != null)
        {
            oneShotAudioSource.PlayOneShot(_startSound, 1f);
            loopingAudioSource.FadeIn(0.2f, false, false, 1f);
        }
    }

    public virtual void Toggle()
    {
        SetOpen(!_isOpen);
    }
}