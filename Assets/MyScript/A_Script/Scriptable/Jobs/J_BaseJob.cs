using System.Collections.Generic;
using UnityEngine;


public class J_BaseJob : ScriptableObject
{
    public List<SkillRequirement> requiredSkills;
    
}

public abstract class J_WorkableJob : J_BaseJob
{
    public int totalProgress;
}
