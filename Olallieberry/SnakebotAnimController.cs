using UnityEngine;

namespace Olallieberry;

[RequireComponent(typeof(Animator))]
public class SnakebotAnimController : MonoBehaviour
{
    public CharacterDialogueTree dialogueTree;

    private Animator _animator;

    private static readonly int Talking = Animator.StringToHash("Talking");
    private static readonly int Address = Animator.StringToHash("Address");

    public void Start()
    {
        _animator = GetComponent<Animator>();

        dialogueTree = GetComponentInChildren<CharacterDialogueTree>();
        dialogueTree.OnStartConversation += OnStartConversation;
        dialogueTree.OnEndConversation += OnEndConversation;
        dialogueTree.OnAdvancePage += OnAdvancePage;
    }

    public void OnDestroy()
    {
        if (dialogueTree == null)
            return;

        dialogueTree.OnStartConversation -= OnStartConversation;
        dialogueTree.OnEndConversation -= OnEndConversation;
        dialogueTree.OnAdvancePage -= OnAdvancePage;
    }

    private void OnStartConversation()
    {
        _animator.SetBool(Talking, true);
    }

    private void OnEndConversation()
    {
        _animator.SetBool(Talking, false);
    }

    private void OnAdvancePage(string nodeName, int pageNum)
    {
        _animator.SetBool(Talking, true);

        if (ShouldNod(nodeName, pageNum))
            _animator.SetTrigger(Address);
    }

    private static bool ShouldNod(string nodeName, int pageNum)
    {
        return (nodeName, pageNum) switch
        {
            ("WATCHING_SORRY", 0) => true, // "It is alright."
            ("WATCHING_WORKED", 0) => true, // "Yes."

            ("WARP", 0) => true, // "Yes."
            ("STILL_ASLEEP", 0) => true, // "Yes."

            ("WORTH_WAKING", 1) => true, // "No. I suppose it doesn't."
            ("REALIZATION", 1) => true, // "Perhaps."

            ("WAKE", 0) => true, // "I think I understand."
            ("WAKE", 3) => true, // "I will wake them."

            ("FINAL", 4) => true, // "Thank you, Hearthian."

            _ => false
        };
    }
}