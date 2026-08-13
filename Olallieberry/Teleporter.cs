using System.Collections.Generic;
using UnityEngine;

namespace Olallieberry;

/// <summary>
/// Teleports players and probes between two connected teleporters.
/// Prevents objects from immediately teleporting back on arrival.
/// </summary>
public class Teleporter : EffectVolume
{
    /// <summary>
    /// The teleporter this teleporter connects to.
    /// </summary>
    [Header("Teleport")]
    [Tooltip("The teleporter this teleporter connects to.")]
    public Teleporter connectedTeleporter;

    /// <summary>
    /// Audio source used when teleporting.
    /// </summary>
    [Header("Audio")]
    [Tooltip("Audio source used when teleporting.")]
    public OWAudioSource audioSource;

    /// <summary>
    /// Sound played when teleporting.
    /// </summary>
    [Tooltip("Sound played when teleporting.")]
    public AudioType teleportSound = AudioType.SingularityOnPlayerEnterExit;

    /// <summary>
    /// Objects blocked from teleporting until they leave this pad.
    /// </summary>
    private readonly HashSet<GameObject> _blockedObjects = new();

    public void OnValidate()
    {
        _triggerVolume = gameObject.GetAddComponent<OWTriggerVolume>();
        _triggerVolume.Reset();
    }

    /// <summary>
    /// Initializes the teleporter and finds its audio source.
    /// </summary>
    public override void Awake()
    {
        base.Awake();

        if (audioSource == null)
            audioSource = GetComponentInChildren<OWAudioSource>();
    }

    /// <summary>
    /// Teleports valid objects that enter the pad.
    /// </summary>
    public override void OnEffectVolumeEnter(GameObject hitObj)
    {
        // Only teleport the player or probe.
        if (!hitObj.CompareTag("PlayerDetector") && !hitObj.CompareTag("ProbeDetector"))
            return;

        // Prevent an immediate return teleport.
        if (_blockedObjects.Contains(hitObj))
            return;

        if (!CanTeleport(hitObj))
            return;

        Teleport(hitObj);
    }

    /// <summary>
    /// Allows an arriving object to use this teleporter after leaving it.
    /// </summary>
    public override void OnEffectVolumeExit(GameObject hitObj)
    {
        _blockedObjects.Remove(hitObj);
    }

    /// <summary>
    /// Returns whether the given object is currently allowed to teleport.
    /// </summary>
    protected virtual bool CanTeleport(GameObject obj)
    {
        return true;
    }

    /// <summary>
    /// Warps an object to the connected teleporter while preserving its
    /// position and rotation relative to the source pad.
    /// </summary>
    public virtual void Teleport(GameObject obj)
    {
        if (obj == null || connectedTeleporter == null)
            return;

        var body = obj.GetAttachedOWRigidbody(false);
        var target = connectedTeleporter.transform;
        var targetBody = target.GetAttachedOWRigidbody(false);

        if (body == null || targetBody == null)
            return;

        // Stop the destination from immediately sending the object back.
        connectedTeleporter.BlockUntilExit(obj);

        // Preserve the object's position and rotation relative to this pad.
        var localPosition = transform.InverseTransformPoint(body.GetPosition());
        var localRotation =
            Quaternion.Inverse(transform.rotation) *
            body.GetRotation();

        // Convert that relative transform into the destination pad's space.
        var targetPosition =
            target.TransformPoint(localPosition);

        var targetRotation =
            target.rotation *
            localRotation;

        // Move the body.
        body.SetRotation(targetRotation);
        body.SetPosition(targetPosition);
        body.SetVelocity(targetBody.GetPointVelocity(targetPosition));

        if (!Physics.autoSyncTransforms)
            Physics.SyncTransforms();

        // Play the teleport sound at both pads.
        PlayTeleportSound();
        connectedTeleporter.PlayTeleportSound();
    }

    /// <summary>
    /// Plays this teleporter's teleport sound.
    /// </summary>
    protected void PlayTeleportSound()
    {
        if (audioSource != null)
            audioSource.PlayOneShot(teleportSound);
    }

    /// <summary>
    /// Blocks an object from teleporting until it exits this pad.
    /// </summary>
    private void BlockUntilExit(GameObject obj)
    {
        _blockedObjects.Add(obj);
    }
}