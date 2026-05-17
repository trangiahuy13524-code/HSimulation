using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Job
{
    public IEnumerable<SkillRequirement> requiredSkills;
    public float currentProgress = 0;
    public float totalProgress = 0;
    public bool reserved;
    //public Pawn worker;
    public Action<Pawn> result;
    public Workable workBuilding;
}