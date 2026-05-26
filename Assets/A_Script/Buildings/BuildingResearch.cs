using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingResearch: BuildingWorkable
{
    

    

    

    public void CreateJob()
    {
        WorkableJob job = new();

        job.requiredSkills = requiredSkills;
        job.workBuilding = this;
        job.totalProgress = 100;
        job.result = (pawn) =>
        {
            Debug.Log("Finish Research");
        };

        jobManager.AddJob(job);
    }

    private void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            CreateJob();
        }
    }
}