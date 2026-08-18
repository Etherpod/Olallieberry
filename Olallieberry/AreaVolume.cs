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
/// Logs whenever an object enters/exits a station area and reveals the
/// corresponding ship log fact when the player enters.
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

        if (!hitObj.CompareTag("PlayerDetector"))
            return;

        string factID = GetShipLogFactID();

        if (!string.IsNullOrWhiteSpace(factID))
            Locator.GetShipLogManager().RevealFact(factID);
    }

    public override void OnEffectVolumeExit(GameObject hitObj)
    {
        Olallieberry.Instance.ModHelper.Console.WriteLine(
            $"[AreaVolume] {hitObj.name} exited {area}.",
            MessageType.Info
        );
    }

    /// <summary>
    /// Gets the ship log fact revealed when the player enters this area.
    /// </summary>
    private string GetShipLogFactID()
    {
        return area switch
        {
            StationArea.Entrance => "OLALLIEBERRY_EPHEMERIS_FOUND",
            StationArea.Hub => "",
            StationArea.Basalt => "OLALLIEBERRY_BASALT_FOUND",
            StationArea.Caves => "OLALLIEBERRY_CAVES_FOUND",
            StationArea.Spikes => "OLALLIEBERRY_SPIKES_FOUND",
            StationArea.Beach => "OLALLIEBERRY_BEACH_FOUND",
            StationArea.Sphere => "OLALLIEBERRY_SPHERE_FOUND",
            StationArea.Shortcut => "OLALLIEBERRY_SHORTCUT_FOUND",
            _ => null
        };
    }
}