using UnityEngine;

namespace Olallieberry;

/// <summary>
/// A lever trigger volume that only activates when the player enters it.
/// </summary>
public class PlayerOnlyLeverTriggerVolume : LeverTriggerVolume
{
    public override bool CheckDetector(GameObject hitObj)
    {
        return hitObj.CompareTag("PlayerDetector");
    }
}