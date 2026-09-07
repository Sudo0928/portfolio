using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;

public class UserDialogue : NetworkBehaviour
{
    private readonly SyncHashSet<int> completedDialogueIds = new();
    public IReadOnlyCollection<int> CompletedDialogueIds => completedDialogueIds.Collection;

    private UserManager userManager;

    public void Init(UserManager userManager, UserDialogueData dialogueData)
    {
        this.userManager = userManager;
        completedDialogueIds.AddRange(dialogueData.completedDialogueIds);
    }

    public void AddCompletedDialogueId(int dialogueId)
    {
        if(completedDialogueIds.Contains(dialogueId)) return;
        completedDialogueIds.Add(dialogueId);
    }

    public void RemoveCompletedDialogueId(int dialogueId)
    {
        if(!completedDialogueIds.Contains(dialogueId)) return;
        completedDialogueIds.Remove(dialogueId);
    }
}