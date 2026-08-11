using UnityEngine;
using System.Collections.Generic;

public class BuildingResearch: BuildingWorkable
{
    public ResearchTree researchTree;

    public override JobBuilding CreateJob(JobDataWorkable data)
    {
        var research = data as DataJobResearch;
        if (research == null)
        {
            Debug.LogWarning("JobData is not JobDataResearch");
            return null;
        }
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