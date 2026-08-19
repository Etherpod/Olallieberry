using Olallieberry.TimeZones;
using UnityEngine;

namespace Olallieberry;

public class BasaltLavaController : TimeZoneObject
{
    [Header("Lava Level")]
    public float lowerLavaHeight = 0.025f;
    public float upperLavaHeight = 0.35f;

    public static readonly float riseSeconds = 35f;
    public static readonly float lowerSeconds = 35f;
    public static readonly float raisedWaitSeconds = 5f;
    public static readonly float loweredWaitSeconds = 5f;

    public LevelState State => _state;

    private LevelState _state = LevelState.Lowering;
    private float _lavaHeight;
    private float _waitTimer;

    protected override void Awake()
    {
        base.Awake();

        _lavaHeight = upperLavaHeight;
        SetLavaHeight(_lavaHeight);
    }

    public void Update()
    {
        if (!IsActive)
            return;

        switch (_state)
        {
            case LevelState.Raised:
                _waitTimer += Time.deltaTime;

                if (_waitTimer >= raisedWaitSeconds)
                {
                    _waitTimer = 0f;
                    _state = LevelState.Lowering;
                }
                break;

            case LevelState.Lowering:
                MoveLava(lowerLavaHeight, lowerSeconds);

                if (_lavaHeight == lowerLavaHeight)
                    _state = LevelState.Lowered;
                break;

            case LevelState.Lowered:
                _waitTimer += Time.deltaTime;

                if (_waitTimer >= loweredWaitSeconds)
                {
                    _waitTimer = 0f;
                    _state = LevelState.Rising;
                }
                break;

            case LevelState.Rising:
                MoveLava(upperLavaHeight, riseSeconds);

                if (_lavaHeight == upperLavaHeight)
                    _state = LevelState.Raised;
                break;
        }

        SetLavaHeight(_lavaHeight);
    }

    private void MoveLava(float targetHeight, float duration)
    {
        if (duration <= 0f)
        {
            _lavaHeight = targetHeight;
            return;
        }

        float distance = upperLavaHeight - lowerLavaHeight;
        float speed = distance / duration;

        _lavaHeight = Mathf.MoveTowards(
            _lavaHeight,
            targetHeight,
            speed * Time.deltaTime);
    }

    protected override void OnZoneReset(TimeZone zone)
    {
        _state = LevelState.Lowering;
        _waitTimer = 0f;
        _lavaHeight = upperLavaHeight;

        SetLavaHeight(_lavaHeight);
    }

    public void SetLavaHeight(float lavaHeight)
    {
        var scale = transform.localScale;
        scale.y = lavaHeight;
        transform.localScale = scale;
    }
}