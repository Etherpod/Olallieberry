using System.Collections.Generic;
using UnityEngine;

namespace Olallieberry;

/// <summary>
/// A sliding door controlled by one or more
/// <see cref="PressurePlate"/> objects.
/// </summary>
public class PressurePlateDoorController : DoorController, IPressurePlateReceiver
{
    public enum ActivationMode
    {
        /// <summary>
        /// Opens when any controlling pressure plate is pressed.
        /// </summary>
        Any,

        /// <summary>
        /// Opens only while every controlling pressure plate is pressed.
        /// </summary>
        All
    }

    /// <summary>
    /// Determines whether any or all pressure plates are required to open the door.
    /// </summary>
    [Header("Activation")]
    [Tooltip("Determines whether any or all pressure plates are required to open the door.")]
    public ActivationMode activationMode = ActivationMode.Any;

    /// <summary>
    /// The number of pressure plates that control this door.
    /// Required when using the <see cref="ActivationMode.All"/> activation mode.
    /// </summary>
    [Tooltip("The number of pressure plates that control this door. Required when using the All activation mode.")]
    [Min(1)]
    public int requiredPlateCount = 1;

    private readonly HashSet<PressurePlate> _activePlates = [];

    /// <inheritdoc/>
    public void SetPressurePlateState(PressurePlate pressurePlate, bool isPressed)
    {
        if (pressurePlate == null)
        {
            return;
        }

        if (isPressed)
        {
            _activePlates.Add(pressurePlate);
        }
        else
        {
            _activePlates.Remove(pressurePlate);
        }

        bool shouldOpen = activationMode switch
        {
            ActivationMode.Any => _activePlates.Count > 0,
            ActivationMode.All => _activePlates.Count >= requiredPlateCount,
            _ => false
        };

        SetOpen(shouldOpen);
    }
}