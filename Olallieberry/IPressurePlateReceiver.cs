using UnityEngine;

namespace Olallieberry;

/// <summary>
/// An object that responds when a <see cref="PressurePlate"/> is
/// pressed or released.
/// </summary>
public interface IPressurePlateReceiver
{
    /// <summary>
    /// Applies the current state of a pressure plate to this receiver.
    /// </summary>
    /// <param name="pressurePlate">
    /// The pressure plate whose state changed.
    /// </param>
    /// <param name="isPressed">
    /// Whether the pressure plate is currently pressed.
    /// </param>
    void SetPressurePlateState(
        PressurePlate pressurePlate,
        bool isPressed
    );
}