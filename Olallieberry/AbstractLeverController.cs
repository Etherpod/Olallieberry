using System;
using UnityEngine;

namespace Olallieberry;

public abstract class AbstractLeverController : MonoBehaviour
{
    /// <summary>
    /// Invoked whenever the lever changes state.
    /// </summary>
    public event Action<bool> OnStateChanged;

    /// <summary>
    /// Whether the lever is currently active.
    /// </summary>
    public bool IsActive => _active;

    private bool _active;

    public void Activate()
    {
        SetActive(true);
    }

    public void Deactivate()
    {
        SetActive(false);
    }

    public void SetActive(bool active)
    {
        if (_active == active)
            return;

        _active = active;
        OnStateChanged?.Invoke(_active);
    }

    public void Toggle()
    {
        SetActive(!_active);
    }
}