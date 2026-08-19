using OWML.Common;
using UnityEngine;

namespace Olallieberry;

/// <summary>
/// Reveals ship log facts when the player or probe enters this effect volume.
/// </summary>
public class ShipLogRevealVolume : EffectVolume
{
    /// <summary>
    /// Ship log facts revealed when the player enters the volume.
    /// </summary>
    [Header("Facts")]
    [Tooltip("Ship log facts revealed when the player enters the volume.")]
    public string[] playerFactIDs;

    /// <summary>
    /// Ship log facts revealed when the probe enters the volume.
    /// </summary>
    [Tooltip("Ship log facts revealed when the probe enters the volume.")]
    public string[] probeFactIDs;

    public void OnValidate()
    {
        _triggerVolume = gameObject.GetAddComponent<OWTriggerVolume>();
        _triggerVolume.Reset();
    }

    public void Start()
    {
        if (playerFactIDs == null)
            playerFactIDs = [];
        if (probeFactIDs == null)
            probeFactIDs = [];

        foreach (string factID in playerFactIDs)
        {
            Olallieberry.Instance.ModHelper.Console.WriteLine(
                $"[ShipLogRevealVolume] {transform.name} can reveal player fact ID: {factID}",
                MessageType.Info
            );
        }

        foreach (string factID in probeFactIDs)
        {
            Olallieberry.Instance.ModHelper.Console.WriteLine(
                $"[ShipLogRevealVolume] {transform.name} can reveal probe fact ID: {factID}",
                MessageType.Info
            );
        }
    }

    /// <summary>
    /// Reveals the appropriate ship log facts when the player or probe enters.
    /// </summary>
    public override void OnEffectVolumeEnter(GameObject hitObj)
    {
        if (hitObj.CompareTag("PlayerDetector"))
            RevealFacts(playerFactIDs);
        else if (hitObj.CompareTag("ProbeDetector"))
            RevealFacts(probeFactIDs);
    }

    public override void OnEffectVolumeExit(GameObject hitObj)
    {
    }

    /// <summary>
    /// Reveals each unrevealed fact in the specified list.
    /// </summary>
    private static void RevealFacts(string[] factIDs)
    {
        ShipLogManager shipLogManager = Locator.GetShipLogManager();

        foreach (string factID in factIDs)
        {
            if (!shipLogManager.IsFactRevealed(factID))
                shipLogManager.RevealFact(factID, true, true);
        }
    }
}