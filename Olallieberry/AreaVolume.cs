using OWML.Common;
using UnityEngine;

namespace Olallieberry;

/// <summary>
/// Identifies the major areas of the station.
/// </summary>
public enum StationArea
{
    None = -1,
    Entrance,
    Hub,
    Basalt,
    Caves,
    Spikes,
    Beach,
    Sphere,
    Shortcut
}

/// <summary>
/// Logs whenever an object enters/exits a station area.
/// </summary>
public class AreaVolume : EffectVolume
{
    /// <summary>
    /// The area represented by this volume.
    /// </summary>
    [Header("Area")]
    [Tooltip("The area represented by this volume.")]
    public StationArea area;

    public void OnValidate()
    {
        _triggerVolume = gameObject.GetAddComponent<OWTriggerVolume>();
        _triggerVolume.Reset();
    }

    public override void OnEffectVolumeEnter(GameObject hitObj)
    {
        Olallieberry.Instance.ModHelper.Console.WriteLine(
            $"[AreaVolume] {hitObj.name} entered {area}.",
            MessageType.Info
        );
    }

    public override void OnEffectVolumeExit(GameObject hitObj)
    {
        Olallieberry.Instance.ModHelper.Console.WriteLine(
            $"[AreaVolume] {hitObj.name} exited {area}.",
            MessageType.Info
        );
    }
}