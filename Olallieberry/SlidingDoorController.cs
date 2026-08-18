using UnityEngine;

namespace Olallieberry;

/// <summary>
/// Controls a door that opens by translating along a local offset.
/// </summary>
public class SlidingDoorController : AbstractDoorController
{
    /// <summary>
    /// The door's open position relative to its initial local position.
    /// </summary>
    [Tooltip("The door's open position relative to its initial local position.")]
    public Vector3 openOffset = new(0f, 3f, 0f);

    /// <summary>
    /// The door's movement speed in units per second.
    /// </summary>
    [Tooltip("The door's movement speed in units per second.")]
    [Min(0f)]
    public float moveSpeed = 3f;

    protected Vector3 _closedLocalPosition;

    protected override void InitializeMovement()
    {
        _closedLocalPosition = movingPart.localPosition;
    }

    protected override bool UpdateMovement()
    {
        Vector3 targetPosition = _isOpen
            ? _closedLocalPosition + openOffset
            : _closedLocalPosition;

        movingPart.localPosition = Vector3.MoveTowards(
            movingPart.localPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(movingPart.localPosition, targetPosition) >= 0.001f)
        {
            return false;
        }

        movingPart.localPosition = targetPosition;
        return true;
    }

    protected override void ApplyOpenImmediate(bool open)
    {
        movingPart.localPosition = open
            ? _closedLocalPosition + openOffset
            : _closedLocalPosition;
    }
}