using System.Collections.Generic;
using UnityEngine;


public class JobDataBase : DataMain
{
    public List<SkillRequirement> requiredSkills;
    
}

public abstract class JobDataWorkable : JobDataBase
{
    public int totalProgress;
}
