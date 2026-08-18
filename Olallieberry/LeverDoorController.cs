using UnityEngine;

namespace Olallieberry;

/// <summary>
/// Opens and closes a door based on the state of a lever.
/// </summary>
public class LeverDoorController : MonoBehaviour
{
    /// <summary>
    /// The lever controlling the door.
    /// </summary>
    [Header("Lever")]
    [Tooltip("The lever controlling the door.")]
    public AbstractLeverController lever;

    /// <summary>
    /// The door controlled by the lever.
    /// </summary>
    [Header("Door")]
    [Tooltip("The door controlled by the lever.")]
    public AbstractDoorController door;

    public void Start()
    {
        if (lever == null)
            lever = GetComponent<AbstractLeverController>();

        if (lever == null)
            return;

        lever.OnStateChanged += OnLeverStateChanged;
        OnLeverStateChanged(lever.IsActive);
    }

    public void OnDestroy()
    {
        if (lever != null)
            lever.OnStateChanged -= OnLeverStateChanged;
    }

    private void OnLeverStateChanged(bool active)
    {
        if (door == null)
            return;

        if (active)
            door.Open();
        else
            door.Close();
    }
}