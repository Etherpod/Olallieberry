using UnityEngine;

namespace Olallieberry;

/// <summary>
/// Opens a specified door while the player is inside this trigger volume.
/// </summary>
public class DoorTriggerVolume : EffectVolume
{
    /// <summary>
    /// The door controlled by this trigger volume.
    /// </summary>
    [Header("Door")]
    [Tooltip("The door controlled by this trigger volume.")]
    public DoorController door;

    public void OnValidate()
    {
        _triggerVolume = gameObject.GetAddComponent<OWTriggerVolume>();
        _triggerVolume.Reset();
    }

    /// <summary>
    /// Opens the door when the player enters.
    /// </summary>
    public override void OnEffectVolumeEnter(GameObject hitObj)
    {
        if (!hitObj.CompareTag("PlayerDetector"))
            return;

        door?.Open();
    }

    /// <summary>
    /// Closes the door when the player leaves.
    /// </summary>
    public override void OnEffectVolumeExit(GameObject hitObj)
    {
        if (!hitObj.CompareTag("PlayerDetector"))
            return;

        door?.Close();
    }
}