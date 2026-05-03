using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Craftable: Workable
{
    public List<Skill> requiredSkills = new();

    public override void PerformWork(Pawn pawn, Job job)
    {
        var work = job.worksToDo;

        if (work.finishedWorks >= work.works.Count)
        {
            FinishJob(pawn, job);
            return;
        }

        work.works[work.finishedWorks].Invoke();
    }

    void FinishJob(Pawn pawn, Job job)
    {
        Debug.Log("Craft finished!");

        pawn.RemoveJob();
    }

    public void CreateJob()
    {
        Job job = new();

        job.requiredSkills = requiredSkills;
        job.workBuilding = this;

        job.worksToDo = new Work(
            new List<Action>
            {
            () => Debug.Log("Crafting step")
            });

        jobManager.AddJob(job);
    }

    protected override void Start()
    {
        base.Start();
    }
}