using System;
using System.Collections;
using System.Collections.Generic;


public class Job
{
    public IEnumerable<SkillRequirement> requiredSkills;
    public float currentProgress = 0;
    public float totalProgress = 0;
    public Action<Pawn> result;
    public Workable workBuilding;
    public bool removed = false;
}