using UnityEngine;

namespace Olallieberry;

/// <summary>
/// A teleporter that only activates while the beach tide is fully raised.
/// </summary>
public class SnakebotTeleporter : Teleporter
{
    /// <summary>
    /// The beach tide controller used to determine whether this teleporter is active.
    /// </summary>
    [Header("Beach")]
    [Tooltip("The beach tide controller used to determine whether this teleporter is active.")]
    public BeachTidesController beachTides;

    /// <summary>
    /// Only allows teleporting while the beach tide is fully raised.
    /// </summary>
    protected override bool CanTeleport(GameObject obj)
    {
#if DEBUG
        return true;
#else
        return beachTides != null && beachTides.State == LevelState.Raised;
#endif
    }
}