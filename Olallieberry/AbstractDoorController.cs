using UnityEngine;

namespace Olallieberry;

/// <summary>
/// Base class for doors that can be opened and closed.
/// Handles shared door state and audio.
/// </summary>
public abstract class AbstractDoorController : MonoBehaviour
{
    /// <summary>
    /// The transform that moves when the door opens.
    /// </summary>
    [Header("Movement")]
    [Tooltip("The transform that moves when the door opens.")]
    public Transform movingPart;

    /// <summary>
    /// Whether the door starts open.
    /// </summary>
    [Tooltip("Whether the door starts open.")]
    public bool startOpen;

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

    protected bool _isOpen;
    protected bool _isMoving;

    public bool IsOpen => _isOpen;

    protected virtual void Awake()
    {
        if (movingPart == null)
        {
            movingPart = transform;
        }

        InitializeMovement();
        SetOpenImmediate(startOpen);
    }

    protected virtual void Start()
    {
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

        if (!UpdateMovement())
        {
            return;
        }

        _isMoving = false;

        if (oneShotAudioSource != null)
        {
            oneShotAudioSource.PlayOneShot(_stopSound, 1f);
        }

        if (loopingAudioSource != null)
        {
            loopingAudioSource.FadeOut(
                0.2f,
                OWAudioSource.FadeOutCompleteAction.STOP,
                0f
            );
        }
    }

    /// <summary>
    /// Initializes the door's movement-specific state.
    /// </summary>
    protected abstract void InitializeMovement();

    /// <summary>
    /// Updates the door's movement toward its current target.
    /// Returns true once the movement has finished.
    /// </summary>
    protected abstract bool UpdateMovement();

    /// <summary>
    /// Immediately applies the requested door state.
    /// </summary>
    protected abstract void ApplyOpenImmediate(bool open);

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

        if (oneShotAudioSource != null)
        {
            oneShotAudioSource.PlayOneShot(_startSound, 1f);
        }

        if (loopingAudioSource != null)
        {
            loopingAudioSource.FadeIn(0.2f, false, false, 1f);
        }
    }

    public virtual void SetOpenImmediate(bool open)
    {
        _isOpen = open;
        _isMoving = false;

        ApplyOpenImmediate(open);

        if (loopingAudioSource != null)
        {
            loopingAudioSource.Stop();
        }
    }

    public virtual void Toggle()
    {
        SetOpen(!_isOpen);
    }
}