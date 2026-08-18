using UnityEngine;

namespace Olallieberry;

/// <summary>
/// Controls a door that opens by rotating around its local Y axis.
/// </summary>
public class RotatingDoorController : AbstractDoorController
{
    /// <summary>
    /// The amount the door rotates when opened.
    /// </summary>
    [Tooltip("The amount the door rotates when opened.")]
    public float openRotation = 180f;

    /// <summary>
    /// The door's opening speed in degrees per second.
    /// </summary>
    [Tooltip("The door's opening speed in degrees per second.")]
    [Min(0f)]
    public float openingSpeed = 60f;

    /// <summary>
    /// The door's closing speed in degrees per second.
    /// </summary>
    [Tooltip("The door's closing speed in degrees per second.")]
    [Min(0f)]
    public float closingSpeed = 60f;

    protected Quaternion _closedLocalRotation;
    protected Quaternion _openLocalRotation;

    protected override void InitializeMovement()
    {
        _closedLocalRotation = movingPart.localRotation;
        _openLocalRotation =
            _closedLocalRotation * Quaternion.Euler(0f, openRotation, 0f);
    }

    protected override bool UpdateMovement()
    {
        Quaternion targetRotation = _isOpen
            ? _openLocalRotation
            : _closedLocalRotation;

        float speed = _isOpen
            ? openingSpeed
            : closingSpeed;

        movingPart.localRotation = Quaternion.RotateTowards(
            movingPart.localRotation,
            targetRotation,
            speed * Time.deltaTime
        );

        if (Quaternion.Angle(movingPart.localRotation, targetRotation) >= 0.01f)
        {
            return false;
        }

        movingPart.localRotation = targetRotation;
        return true;
    }

    protected override void ApplyOpenImmediate(bool open)
    {
        movingPart.localRotation = open
            ? _openLocalRotation
            : _closedLocalRotation;
    }
}