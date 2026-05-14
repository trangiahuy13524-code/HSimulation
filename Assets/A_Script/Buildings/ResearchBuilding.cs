using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ResearchBuilding: Workable
{
    public List<SkillRequirement> requiredSkills = new();

    

    

    public void CreateJob()
    {
        Job job = new();

        job.requiredSkills = requiredSkills;
        job.workBuilding = this;
        job.totalProgress = 100;
        job.result = (pawn) =>
        {
            Debug.Log("Finish Research");
        };

        jobManager.AddJob(job);
    }

    //public override void DoTask(Pawn pawn, string taskName)
    //{
    //    if (taskName == "Gotobuilding" && !pawn.ReachWorkPos)
    //    {
    //        WorkPosition wP = GetAvailableWorkPos();
    //        if (wP == null)
    //        {
    //            pawn.ReturnJob();
    //            return;
    //        }

    //        if (!world.IsPositionPathValid(wP.workPos))
    //        {
    //            if (wP.workPos != pawn.CurrentGridPosition)
    //            {
    //                pawn.ReturnJob();
    //                return;
    //            }
    //        }
    //        Debug.Log("Do Job");

    //        pawn.MakePath(wP.workPos);
    //        wP.occupied = true;
    //    }
    //}

    //static readonly WaitForSeconds workDelay =
    //new WaitForSeconds(1f);
    protected override IEnumerator WorkRoutine(Pawn pawn, WorkPosition wP)
    {
        yield return pawn.MoveTo(wP.workPos);
        if (pawn.lastWorkResult != WorkResult.Success)
            yield break;

        yield return pawn.DoProgressWork(workDirection);
    }



    protected override void Start()
    {
        base.Start();
        CreateJob();
    }

    private void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            CreateJob();
        }
    }
}