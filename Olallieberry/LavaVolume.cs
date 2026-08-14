using UnityEngine;

namespace Olallieberry;

/// <summary>
/// Kills the player or retrieves the probe when they enter lava.
/// </summary>
public class LavaVolume : EffectVolume
{
    public void OnValidate()
    {
        _triggerVolume = gameObject.GetAddComponent<OWTriggerVolume>();
        _triggerVolume.Reset();
    }

    public override void OnEffectVolumeEnter(GameObject hitObj)
    {
        if (hitObj.CompareTag("PlayerDetector"))
        {
            Locator.GetDeathManager().KillPlayer(DeathType.Lava);
        }
        else if (hitObj.CompareTag("ProbeDetector"))
        {
            hitObj.GetAttachedOWRigidbody(false)
                .GetRequiredComponent<SurveyorProbe>()
                .ExternalRetrieve(false);
        }
    }

    public override void OnEffectVolumeExit(GameObject hitObj)
    {
    }
}