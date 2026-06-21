using UnityEngine;
using System.Collections.Generic;

public class BuildingResearch: BuildingWorkable
{
    public ResearchTree researchTree;

    public JobResearch CreateJob(ResearchData research)
    {
        JobResearch job = new();

        job.requiredSkills = research.requiredSkills;
        job.researchData = research;
        job.workBuilding = this;
        job.result = (pawn) =>
        {
            Debug.Log("Finish Research");
        };

        jobManager.AddJob(job);

        return job;
    }
}