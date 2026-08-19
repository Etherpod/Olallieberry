using UnityEngine;

namespace Olallieberry;

/// <summary>
/// Displays the current Hexagon Beach tide level using a physical meter bar.
/// </summary>
public class BeachTideMeter : MonoBehaviour
{
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

    public void Awake()
    {
        if (fill != null)
            _fullScale = fill.localScale;
    }

    public void Start()
    {
        if (tides == null)
            tides = FindObjectOfType<BeachTidesController>();
    }

    public void LateUpdate()
    {
        if (tides == null || fill == null)
            return;

        Vector3 scale = _fullScale;
        scale.y *= Mathf.Lerp(minimumFill, 1f, tides.TideLevel);

        fill.localScale = scale;
    }
}