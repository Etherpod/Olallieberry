using UnityEngine;

namespace Olallieberry;

/// <summary>
/// A door trigger volume that only activates when the player enters it.
/// </summary>
public class PlayerOnlyDoorTriggerVolume : DoorTriggerVolume
{
    public override bool CheckDetector(GameObject hitObj)
    {
        return hitObj.CompareTag("PlayerDetector");
    }
}