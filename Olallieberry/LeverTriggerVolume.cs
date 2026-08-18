using UnityEngine;

namespace Olallieberry;

/// <summary>
/// Activates a lever when a valid detector enters this trigger volume.
/// </summary>
public class LeverTriggerVolume : EffectVolume
{
    /// <summary>
    /// The lever activated by this trigger volume.
    /// </summary>
    [Header("Lever")]
    [Tooltip("The lever activated by this trigger volume.")]
    public AbstractLeverController lever;

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
    /// Activates the lever when a valid detector enters.
    /// </summary>
    public override void OnEffectVolumeEnter(GameObject hitObj)
    {
        if (!CheckDetector(hitObj))
            return;

        lever.Activate();
    }

    public override void OnEffectVolumeExit(GameObject hitObj)
    {

    }
}