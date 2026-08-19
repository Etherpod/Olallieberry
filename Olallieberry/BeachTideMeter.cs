using UnityEngine;

namespace Olallieberry;

/// <summary>
/// Displays the current Hexagon Beach tide level using a physical meter bar.
/// Reveals information about the meter when the player approaches it.
/// </summary>
public class BeachTideMeter : EffectVolume
{
    private static readonly string exploreFactID = "OLALLIEBERRY_SHORTCUT_TIDE_METER";
    private static readonly string rumorFactID = "OLALLIEBERRY_SHORTCUT_TIDE_METER_RUMOR";

    /// <summary>
    /// The beach tide controller being monitored.
    /// </summary>
    [Header("Tide")]
    [Tooltip("The beach tide controller being monitored.")]
    public BeachTidesController tides;

    /// <summary>
    /// The transform used as the meter's fill bar.
    /// Its pivot should be at the bottom.
    /// </summary>
    [Header("Meter")]
    [Tooltip("The transform used as the meter's fill bar. Its pivot should be at the bottom.")]
    public Transform fill;

    /// <summary>
    /// Minimum scale of the fill bar when the tide is fully lowered.
    /// </summary>
    [Tooltip("Minimum scale of the fill bar when the tide is fully lowered.")]
    [Min(0f)]
    public float minimumFill = 0.01f;

    private Vector3 _fullScale;

    private bool _playerInside;
    private float _lastTideLevel;
    private bool _factRevealed;
    private bool _rumorRevealed;

    public void OnValidate()
    {
        _triggerVolume = gameObject.GetAddComponent<OWTriggerVolume>();
        _triggerVolume.Reset();
    }

    public void Start()
    {
        if (fill != null)
            _fullScale = fill.localScale;

        if (tides == null)
            tides = FindObjectOfType<BeachTidesController>();

        if (tides != null)
            _lastTideLevel = tides.TideLevel;
    }

    public void LateUpdate()
    {
        if (tides == null)
            return;

        float tideLevel = tides.TideLevel;
        bool tideMoving = !Mathf.Approximately(tideLevel, _lastTideLevel);

        if (fill != null)
        {
            Vector3 scale = _fullScale;
            scale.y *= Mathf.Lerp(minimumFill, 1f, tideLevel);
            fill.localScale = scale;
        }

        if (_playerInside && tideMoving)
            RevealFact();

        _lastTideLevel = tideLevel;
    }

    public override void OnEffectVolumeEnter(GameObject hitObj)
    {
        if (!hitObj.CompareTag("PlayerDetector"))
            return;

        _playerInside = true;

        RevealRumor();

        bool tideMoving =
            tides != null &&
            !Mathf.Approximately(tides.TideLevel, _lastTideLevel);

        if (tideMoving)
            RevealFact();
    }

    public override void OnEffectVolumeExit(GameObject hitObj)
    {
        if (hitObj.CompareTag("PlayerDetector"))
            _playerInside = false;
    }

    private void RevealFact()
    {
        if (_factRevealed)
            return;

        Locator.GetShipLogManager().RevealFact(exploreFactID, true, true);
        _factRevealed = true;
    }

    private void RevealRumor()
    {
        if (_rumorRevealed)
            return;

        Locator.GetShipLogManager().RevealFact(rumorFactID, true, true);
        _rumorRevealed = true;
    }
}