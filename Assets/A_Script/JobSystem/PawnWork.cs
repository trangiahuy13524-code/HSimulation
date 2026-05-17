using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Pawn : WorldObject
{
    Dictionary<Skill, byte> pawnSkills = new();
    [SerializeField] ProgressBar progressBarPrefab;
    ProgressBar progressBarInstance;

    public bool QualifyForSkills(IEnumerable<SkillRequirement> requiredSkills)
    {
        foreach (var required in requiredSkills)
        {
            if (!pawnSkills.TryGetValue(required.skillRef, out byte level))
                return false;

            if (level < required.level)
                return false;
        }

        return true;
    }

    bool reachDestination = false;
    bool destinationInvalid = false;
    Job currentJob;
    WorkPosition workPos;
    public void SetWorkPosition(WorkPosition wP)
    {
        workPos = wP;
    }

    public bool TryFindJob()
    {
        if (currentJob != null)
        {
            return false;
        }

        Job job = jobManager.GetJob(this);


        if (job != null)
        {
            currentJob = job;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void RemoveJob()
    {
        currentState = PawnState.Idle;
        jobManager.RemoveJob(currentJob);
        currentJob = null;
        ClearWorkPosition();
    }
    public void ReturnJob()
    {
        if (currentJob == null)
            return;
        currentState = PawnState.Idle;
        jobManager.ReturnJob(currentJob);

        currentJob = null;
        ClearWorkPosition();
    }
    void ClearWorkPosition()
    {
        if (workPos != null)
        {
            workPos.occupied = false;
            workPos = null;
        }
    }

    bool IsCurrentJobStillValid()
    {
        if (currentJob != null)
        {
            if (currentJob.workBuilding == null)
            {
                RemoveJob();
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return true;
        }
    }

    public WorkResult lastWorkResult;
    public IEnumerator MoveTo(Vector2Int targetPos)
    {
        lastWorkResult = WorkResult.Failed;

        MakePath(targetPos);

        while (!reachDestination)
        {
            if (currentJob == null || currentJob.workBuilding == null)
            {
                lastWorkResult = WorkResult.Cancelled;
                yield break;
            }
            yield return null;
        }

        if (destinationInvalid)
        {
            lastWorkResult = WorkResult.Failed;
            yield break;
        }

        lastWorkResult = WorkResult.Success;
    }

    public IEnumerator DoProgressWork(Direction workDirection)
    {
        lastWorkResult = WorkResult.Failed;
        ChangeDirection(workDirection);

        progressBarInstance = Instantiate(progressBarPrefab, WorldCanvasUI.Instance.transform);
        progressBarInstance.Setup(transform, 0.5f);

        while (currentJob != null &&
               currentJob.workBuilding != null &&
               currentJob.currentProgress < currentJob.totalProgress)
        {
            currentJob.currentProgress += Time.deltaTime * 20;
            progressBarInstance.SetProgress(currentJob.currentProgress / currentJob.totalProgress);
            yield return null;
        }

        Destroy(progressBarInstance.gameObject);
        if (currentJob == null || currentJob.workBuilding == null)
        {
            lastWorkResult = WorkResult.Cancelled;
            yield break;
        }

        lastWorkResult = WorkResult.Success;
    }
    public IEnumerator FinishWork()
    {
        currentJob.result(this);
        yield break;
    }
}