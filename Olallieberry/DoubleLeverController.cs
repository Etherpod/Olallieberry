using System;
using UnityEngine;

namespace Olallieberry;

public class DoubleLeverController : MonoBehaviour
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

    /// <summary>
    /// Invoked whenever the double lever changes state.
    /// </summary>
    public event Action<bool> OnStateChanged;

    /// <summary>
    /// Whether both levers are currently active.
    /// </summary>
    public bool IsActive => _active;

    private bool _active;

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
        bool active =
            firstLever != null &&
            secondLever != null &&
            firstLever.IsActive &&
            secondLever.IsActive;

        if (_active == active)
            return;

        _active = active;
        OnStateChanged?.Invoke(_active);
    }
}