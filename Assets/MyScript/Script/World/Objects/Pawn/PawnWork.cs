using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public partial class Pawn
{
    

    [Header("Pawn Work")]
    private Dictionary<DataSkill, byte> pawnSkills = new();
    
    private bool reachDestination;
    private bool destinationInvalid;
    private JobBase currentJob;
    private CancellationTokenSource jobCTS;

    public bool QualifyForSkills(IEnumerable<SkillRequirement> requiredSkills)
    {
        foreach (SkillRequirement required in requiredSkills)
        {
            if (!pawnSkills.TryGetValue(required.skillRef, out byte level))
            {
                return false;
            }

            if (level < required.level)
            {
                return false;
            }
        }
        return true;
    }

    public async UniTask<bool> TryFindJob(CancellationToken token)
    {
        if (currentJob != null || jobRunning) return false;

        JobBase job = await jobManager.GetJob(this, token);
        if (job == null) return false;

        currentJob = job;
        return true;
    }

    private bool jobRunning;
    private const int MINIMUM_PICKUP_AMOUNT = 1;

    public void RemoveJob()
    {
        jobCTS?.Cancel();
        currentJob.ReturnJob(this);
        currentJob.reserved = false;
        jobManager.RemoveJob(currentJob);
        currentJob = null;
        currentState = PawnState.Idle;
    }

    public void ReturnJob()
    {
        if (currentJob == null) return;

        jobCTS?.Cancel();
        PathReset();
        currentState = PawnState.Idle;
        jobManager.ReturnJob(currentJob, this);
        currentJob = null;
    }

    private async UniTask RunCurrentJob(JobBase job, CancellationTokenSource cancellationSource)
    {
        jobRunning = true;
        try
        {
            await job.DoJob(this, cancellationSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Cancellation returns the unfinished job for another attempt.
        }
        finally
        {
            if (currentJob == job)
            {
                ReturnJob();
            }

            if (jobCTS == cancellationSource)
            {
                jobCTS = null;
            }

            cancellationSource.Dispose();
            jobRunning = false;
        }
    }

    private bool IsCurrentJobStillValid()
    {
        if (currentJob == null || !currentJob.removed) return true;

        RemoveJob();
        return false;
    }

    private void ValidateCurrentJob(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (currentJob == null || currentJob.removed)
        {
            throw new OperationCanceledException(token);
        }
    }

    private async UniTask WaitForWorkDestination(CancellationToken token)
    {
        ValidateCurrentJob(token);
        while (!reachDestination && !destinationInvalid)
        {
            await UniTask.Yield(token);
            ValidateCurrentJob(token);
        }

        if (destinationInvalid)
        {
            throw new OperationCanceledException(token);
        }
    }

    public async UniTask MoveTo(Vector2Int targetPos, CancellationToken token)
    {
        ValidateCurrentJob(token);
        reachDestination = false;
        destinationInvalid = false;
        await MakePath(targetPos, PawnManager.Instance.GetWTS(), token);
        await WaitForWorkDestination(token);
    }

    public async UniTask<int> MoveToAndPickUp(Item item, int amount, CancellationToken token)
    {
        ValidateCurrentJob(token);
        reachDestination = false;
        destinationInvalid = false;
        item.reservingObject = this;
        bool pickedUp = false;

        try
        {
            await MakePathWithoutLast(item.CurrentGridPosition, PawnManager.Instance.GetWTS(), token);
            ValidateCurrentJob(token);

            while (!reachDestination && !destinationInvalid)
            {
                if (item == null)
                {
                    return 0;
                }

                await UniTask.Yield(token);
                ValidateCurrentJob(token);
            }

            if (destinationInvalid)
            {
                throw new OperationCanceledException(token);
            }

            if (item == null || item.CurrentGridPosition != oldDestination)
            {
                return 0;
            }

            int takenAmount = Mathf.Min(Mathf.Max(amount, MINIMUM_PICKUP_AMOUNT), item.StackCount);
            if (takenAmount <= 0)
            {
                throw new OperationCanceledException(token);
            }

            takenAmount = HoldItem(item, takenAmount);
            pickedUp = takenAmount > 0;
            return takenAmount;
        }
        finally
        {
            if (!pickedUp && item != null)
            {
                item.reservingObject = null;
            }
        }
    }

    public async UniTask DoProgressWork(Direction workDirection, CancellationToken token)
    {
        ValidateCurrentJob(token);
        JobBase job = currentJob;
        ChangeDirection(workDirection);
        ProgressBar progressBar = CreateProgressBar();

        try
        {
            while (job.currentProgress < job.totalProgress)
            {
                ValidateCurrentJob(token);
                job.currentProgress += Time.deltaTime * WORK_PROGRESS_PER_SECOND;
                progressBar.SetProgress(Mathf.Clamp01(job.currentProgress / job.totalProgress));
                await UniTask.Yield(token);
            }

            ValidateCurrentJob(token);
        }
        finally
        {
            DestroyProgressBar(progressBar);
        }
    }

    public async UniTask DoResearch(
        DataJobResearch researchData,
        Direction workDirection,
        CancellationToken token)
    {
        ValidateCurrentJob(token);
        ChangeDirection(workDirection);
        var state = researchManager.GetState(researchData);
        ProgressBar progressBar = CreateProgressBar();

        try
        {
            while (!state.completed)
            {
                ValidateCurrentJob(token);
                researchManager.AddProgress(researchData, Time.deltaTime * WORK_PROGRESS_PER_SECOND);
                progressBar.SetProgress(Mathf.Clamp01(state.progress / researchData.totalProgress));
                await UniTask.Yield(token);
            }

            ValidateCurrentJob(token);
        }
        finally
        {
            DestroyProgressBar(progressBar);
        }
    }
}
