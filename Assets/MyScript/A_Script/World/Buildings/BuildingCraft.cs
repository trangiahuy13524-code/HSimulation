using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingCraft : BuildingWorkable
{
    public List<JobDataCraft> jobs;

    // =====================================================
    // FIND NEAREST REQUIRED ITEM
    // =====================================================

    

    // =====================================================
    // JOB
    // =====================================================

    public override JobBuilding CreateJob(JobDataWorkable data)
    {
        var data2 = data as JobDataCraft;
        JobCraft job = new();

        job.requiredSkills = data2.requiredSkills;
        job.recipeData = data2.recipeData;
        job.totalProgress = data2.totalProgress;

        job.workBuilding = this;
        job.result = (pawn) =>
        {
            Debug.Log("Finish Craft");
        };

        jobManager.AddJob(job);
        return job;
    }
}