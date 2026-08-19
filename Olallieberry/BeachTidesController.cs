using Olallieberry.TimeZones;
using UnityEngine;

namespace Olallieberry;

public class BeachTidesController : TimeZoneObject
{
    [Header("Water Level")]
    public float lowerWaterHeight = 0.015f;
    public float upperWaterHeight = 0.65f;

    public static readonly float riseSeconds = 25f;
    public static readonly float lowerSeconds = 25f;
    public static readonly float raisedWaitSeconds = 5f;
    public static readonly float loweredWaitSeconds = 5f;

    public LevelState State => _state;
    public float TideLevel => Mathf.InverseLerp(
        lowerWaterHeight,
        upperWaterHeight,
        _waterHeight
    );

    private LevelState _state = LevelState.Lowered;
    private float _waterHeight;
    private float _waitTimer;
    private bool _wasPlayerInside;

    protected override void Awake()
    {
        base.Awake();

        _waterHeight = lowerWaterHeight;
        SetWaterHeight(_waterHeight);

        if (_timeZone != null)
            _wasPlayerInside = _timeZone.IsPlayerInside;
    }

    public void Update()
    {
        UpdateShipLogFacts();

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

    private void UpdateShipLogFacts()
    {
        if (_timeZone == null)
            return;

        bool playerInside = _timeZone.IsPlayerInside;

        if (_wasPlayerInside && !playerInside)
        {
            string factID = _timeZone.IsProbeInside
                ? "OLALLIEBERRY_BEACH_PROBE"
                : "OLALLIEBERRY_BEACH_RESET";

            Locator.GetShipLogManager().RevealFact(factID);
        }

        _wasPlayerInside = playerInside;
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

    protected override void OnZoneReset(TimeZone zone)
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