using System.Collections.Generic;
using UnityEngine;

namespace Olallieberry;

/// <summary>
/// Opens a specified door while a valid detector is inside this trigger volume.
/// </summary>
public class DoorTriggerVolume : EffectVolume
{
    /// <summary>
    /// The door controlled by this trigger volume.
    /// </summary>
    [Header("Door")]
    [Tooltip("The door controlled by this trigger volume.")]
    public DoorController door;

    [Tooltip("If enabled, the door stays open permanently after being triggered once.")]
    public bool stayOpen;

    private readonly HashSet<GameObject> _detectors = [];

    public void OnValidate()
    {
        _triggerVolume = gameObject.GetAddComponent<OWTriggerVolume>();
        _triggerVolume.Reset();
    }

    /// <summary>
    /// Checks whether the specified object should activate this trigger.
    /// </summary>
    public virtual bool CheckDetector(GameObject hitObj)
    {
        return hitObj.CompareTag("PlayerDetector") || hitObj.CompareTag("ProbeDetector");
    }

    /// <summary>
    /// Opens the door when a valid detector enters.
    /// </summary>
    public override void OnEffectVolumeEnter(GameObject hitObj)
    {
        if (!CheckDetector(hitObj))
            return;

        _detectors.Add(hitObj);
        door.Open();
    }

    /// <summary>
    /// Closes the door when the last valid detector leaves, unless it should stay open.
    /// </summary>
    public override void OnEffectVolumeExit(GameObject hitObj)
    {
        if (!CheckDetector(hitObj))
            return;

        _detectors.Remove(hitObj);

        if (!stayOpen && _detectors.Count == 0)
            door.Close();
    }
}