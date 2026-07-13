using System.Collections.Generic;
using UnityEngine;


public class JobDataBase : ScriptableObject
{
    public List<SkillRequirement> requiredSkills;
    
}

public abstract class JobDataWorkable : JobDataBase
{
    public int totalProgress;
}
