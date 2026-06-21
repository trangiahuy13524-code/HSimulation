using UnityEngine;

public class ScriptableObjectNeedResearch : ScriptableObject
{
    public ResearchData requiredResearch;

    public bool IsUnlocked()
    {
        if (requiredResearch == null)
            return true;

        return ResearchManager.Instance
            .IsCompleted(requiredResearch);
    }
}
