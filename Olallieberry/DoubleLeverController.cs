using UnityEngine;

namespace Olallieberry;

public class DoubleLeverController : AbstractLeverController
{
    /// <summary>
    /// The first lever required to activate this double lever.
    /// </summary>
    [Header("Levers")]
    [Tooltip("The first lever required to activate this double lever.")]
    public SingleLeverController firstLever;

    /// <summary>
    /// The second lever required to activate this double lever.
    /// </summary>
    [Tooltip("The second lever required to activate this double lever.")]
    public SingleLeverController secondLever;

    public void Start()
    {
        if (firstLever != null)
            firstLever.OnStateChanged += OnLeverStateChanged;

        if (secondLever != null)
            secondLever.OnStateChanged += OnLeverStateChanged;

        UpdateState();
    }

    public void OnDestroy()
    {
        if (firstLever != null)
            firstLever.OnStateChanged -= OnLeverStateChanged;

        if (secondLever != null)
            secondLever.OnStateChanged -= OnLeverStateChanged;
    }

    private void OnLeverStateChanged(bool active)
    {
        UpdateState();
    }

    private void UpdateState()
    {
        SetActive(
            firstLever != null &&
            secondLever != null &&
            firstLever.IsActive &&
            secondLever.IsActive
        );
    }
}