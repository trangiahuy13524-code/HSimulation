using UnityEngine;

public class ObjectDataNeedResearch : ScriptableObject
{
    public DataJobResearch requiredResearch;

    public bool IsUnlocked()
    {
        if (requiredResearch == null)
            return true;

        return ResearchManager.Instance
            .IsCompleted(requiredResearch);
    }
}
