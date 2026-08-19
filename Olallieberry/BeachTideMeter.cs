using UnityEngine;

namespace Olallieberry;

/// <summary>
/// Displays the current Hexagon Beach tide level using a physical meter bar.
/// </summary>
public class BeachTideMeter : MonoBehaviour
{
    private const string FactID = "OLALLIEBERRY_SHORTCUT_TIDE_METER";

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
    private OWTriggerVolume _triggerVolume;
    private bool _playerInside;
    private float _lastTideLevel;
    private bool _factRevealed;

    public void Awake()
    {
        if (fill != null)
            _fullScale = fill.localScale;

        _triggerVolume = GetComponentInChildren<OWTriggerVolume>();
    }

    public void Start()
    {
        if (tides == null)
            tides = FindObjectOfType<BeachTidesController>();

        if (tides != null)
            _lastTideLevel = tides.TideLevel;

        if (_triggerVolume != null)
        {
            _triggerVolume.OnEntry += OnEntry;
            _triggerVolume.OnExit += OnExit;
        }
    }

    public void OnDestroy()
    {
        if (_triggerVolume != null)
        {
            _triggerVolume.OnEntry -= OnEntry;
            _triggerVolume.OnExit -= OnExit;
        }
    }

    public void LateUpdate()
    {
        if (tides == null || fill == null)
            return;

        float tideLevel = tides.TideLevel;

        Vector3 scale = _fullScale;
        scale.y *= Mathf.Lerp(minimumFill, 1f, tideLevel);
        fill.localScale = scale;

        if (!_factRevealed &&
            _playerInside &&
            !Mathf.Approximately(tideLevel, _lastTideLevel))
        {
            Locator.GetShipLogManager().RevealFact(FactID, true, true);
            _factRevealed = true;
        }

        _lastTideLevel = tideLevel;
    }

    private void OnEntry(GameObject hitObj)
    {
        if (hitObj.CompareTag("PlayerDetector"))
            _playerInside = true;
    }

    private void OnExit(GameObject hitObj)
    {
        if (hitObj.CompareTag("PlayerDetector"))
            _playerInside = false;
    }
}