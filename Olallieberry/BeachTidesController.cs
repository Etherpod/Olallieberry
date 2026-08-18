using Olallieberry.TimeZones;
using UnityEngine;

namespace Olallieberry;

public class BeachTidesController : TimeZoneObject
{
    [Header("Water Level")]
    public float lowerWaterHeight = 0.015f;
    public float upperWaterHeight = 0.65f;

    [Header("Timing")]
    [Min(0f)]
    public float riseSeconds = 60f;

    [Min(0f)]
    public float lowerSeconds = 60f;

    [Min(0f)]
    public float raisedWaitSeconds = 10f;

    [Min(0f)]
    public float loweredWaitSeconds = 10f;

    public LevelState State => _state;

    private LevelState _state = LevelState.Lowered;
    private float _waterHeight;
    private float _waitTimer;

    protected override void Awake()
    {
        base.Awake();

        _waterHeight = lowerWaterHeight;
        SetWaterHeight(_waterHeight);
    }

    public void Update()
    {
        if (!IsActive)
            return;

        switch (_state)
        {
            case LevelState.Lowered:
                _waitTimer += Time.deltaTime;

                if (_waitTimer >= loweredWaitSeconds)
                {
                    _waitTimer = 0f;
                    _state = LevelState.Rising;
                }
                break;

            case LevelState.Rising:
                MoveWater(upperWaterHeight, riseSeconds);

                if (_waterHeight == upperWaterHeight)
                    _state = LevelState.Raised;
                break;

            case LevelState.Raised:
                _waitTimer += Time.deltaTime;

                if (_waitTimer >= raisedWaitSeconds)
                {
                    _waitTimer = 0f;
                    _state = LevelState.Lowering;
                }
                break;

            case LevelState.Lowering:
                MoveWater(lowerWaterHeight, lowerSeconds);

                if (_waterHeight == lowerWaterHeight)
                    _state = LevelState.Lowered;
                break;
        }

        SetWaterHeight(_waterHeight);
    }

    private void MoveWater(float targetHeight, float duration)
    {
        if (duration <= 0f)
        {
            _waterHeight = targetHeight;
            return;
        }

        float distance = upperWaterHeight - lowerWaterHeight;
        float speed = distance / duration;

        _waterHeight = Mathf.MoveTowards(
            _waterHeight,
            targetHeight,
            speed * Time.deltaTime);
    }

    protected override void OnZoneActivated(TimeZone zone)
    {
    }

    protected override void OnZoneDeactivated(TimeZone zone)
    {
        _state = LevelState.Lowered;
        _waitTimer = 0f;
        _waterHeight = lowerWaterHeight;

        SetWaterHeight(_waterHeight);
    }

    public void SetWaterHeight(float waterHeight)
    {
        var scale = transform.localScale;
        scale.y = waterHeight;
        transform.localScale = scale;
    }
}