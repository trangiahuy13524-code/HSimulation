using System;
using System.Collections.Generic;

[Serializable]
public class Job
{
    public IEnumerable<Skill> requiredSkills;
    public Work worksToDo;
    public Workable workBuilding;
}