using System.Collections.Generic;
using UnityEngine;


public class DataJobBase : DataMain
{
    public List<SkillRequirement> requiredSkills;
    
}

public abstract class DataJobWorkable : DataJobBase
{
    public int totalProgress;
}
