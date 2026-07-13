using UnityEngine;

public class DataObjectNeedResearch : ScriptableObject
{
    public JobDataResearch requiredResearch;

    public bool IsUnlocked()
    {
        if (requiredResearch == null)
            return true;

        return ResearchManager.Instance
            .IsCompleted(requiredResearch);
    }
}
