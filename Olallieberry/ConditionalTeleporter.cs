using UnityEngine;

namespace Olallieberry;

/// <summary>
/// A teleporter that only activates when a dialogue condition is met.
/// </summary>
public class ConditionalTeleporter : Teleporter
{
    /// <summary>
    /// Dialogue condition required to activate the teleporter.
    /// </summary>
    [Header("Condition")]
    [Tooltip("Dialogue condition required to activate the teleporter.")]
    public string dialogueCondition;

    private bool _conditionMet;

    public override void Awake()
    {
        base.Awake();

        // Starts listening for dialogue condition changes.
        GlobalMessenger<string, bool>.AddListener(
            "DialogueConditionChanged",
            OnConditionChanged
        );

#if DEBUG
        _conditionMet = true;
#endif
    }

    public override void OnDestroy()
    {
        // Stops listening for dialogue condition changes.
        GlobalMessenger<string, bool>.RemoveListener(
            "DialogueConditionChanged",
            OnConditionChanged
        );

        base.OnDestroy();
    }

    private void OnConditionChanged(string conditionName, bool conditionState)
    {
        if (conditionName == dialogueCondition)
            _conditionMet = conditionState;
    }

    /// <summary>
    /// Only allows teleporting while the required condition is met.
    /// </summary>
    protected override bool CanTeleport(GameObject obj)
    {
        return _conditionMet;
    }
}