using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingCraft : BuildingWorkable
{
    public List<J_CraftJob> jobs;

    // =====================================================
    // FIND NEAREST REQUIRED ITEM
    // =====================================================

    

    // =====================================================
    // JOB
    // =====================================================

    public void CreateJob(J_CraftJob data)
    {
        JobCraft job = new();

        job.requiredSkills = data.requiredSkills;
        job.recipeData = data.recipeData;
        job.totalProgress = data.totalProgress;

        job.workBuilding = this;
        job.result = (pawn) =>
        {
            Debug.Log("Finish Craft");
        };

        jobManager.AddJob(job);
    }

    private void Update()
    {
        if (Keyboard.current[Key.P].wasPressedThisFrame)
        {
            CreateJob(jobs[0]);
        }
    }
}