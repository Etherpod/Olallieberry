using Olallieberry.TimeZones;
using UnityEngine;

namespace Olallieberry;

public class BasaltLavaController : TimeZoneObject
{
    [Header("Lava Level")]
    public float lowerLavaHeight = 0.025f;
    public float upperLavaHeight = 0.35f;

    [Header("Timing")]
    [Min(0f)]
    public float riseSeconds = 60f;

    [Min(0f)]
    public float lowerSeconds = 60f;

    [Min(0f)]
    public float loweredWaitSeconds = 10f;

    private float _lavaHeight;
    private float _waitTimer;
    private bool _rising;
    private bool _active;

    protected override void Awake()
    {
        base.Awake();

        _lavaHeight = upperLavaHeight;
        SetLavaHeight(_lavaHeight);
    }

    public void Update()
    {
        if (!_active)
            return;

        if (!_rising && _lavaHeight == lowerLavaHeight)
        {
            _waitTimer += Time.deltaTime;

            if (_waitTimer >= loweredWaitSeconds)
            {
                _waitTimer = 0f;
                _rising = true;
            }

            return;
        }

        float targetHeight = _rising ? upperLavaHeight : lowerLavaHeight;
        float duration = _rising ? riseSeconds : lowerSeconds;
        float distance = upperLavaHeight - lowerLavaHeight;
        float speed = distance / duration;

        _lavaHeight = Mathf.MoveTowards(
            _lavaHeight,
            targetHeight,
            speed * Time.deltaTime);

        if (_lavaHeight == targetHeight)
            _rising = !_rising;

        SetLavaHeight(_lavaHeight);
    }

    protected override void OnZoneActivated(TimeZone zone)
    {
        _active = true;
    }

    protected override void OnZoneDeactivated(TimeZone zone)
    {
        _active = false;
        _rising = false;
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