using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public partial class Pawn
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

    public async UniTask<bool> TryFindJob(CancellationToken token)
    {
        if (currentJob != null)
        {
            return false;
        }

        Job job = await jobManager.GetJob(this, token);


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
            if (currentJob.removed)
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

    CancellationTokenSource jobCTS;
    public async UniTask<ActionResult> MoveTo(
    Vector2Int targetPos,
    CancellationToken token)
    {
        reachDestination = false;
        destinationInvalid = false;
        await MakePath(targetPos);

        while (!reachDestination)
        {
            token.ThrowIfCancellationRequested();

            if (currentJob == null || currentJob.removed)
                return ActionResult.Cancelled;

            await UniTask.Yield(token);
        }

        if (destinationInvalid)
            return ActionResult.Cancelled;

        return ActionResult.Success;
    }
    public async UniTask<(ActionResult, int)> MoveToAndPickUp(Item item, int amount, CancellationToken token)
    {
        
        reachDestination = false;
        destinationInvalid = false;

        await MakePathWithoutLast(item.CurrentGridPosition);
        while (!reachDestination)
        {
            token.ThrowIfCancellationRequested();

            if (currentJob == null || currentJob.removed)
                return (ActionResult.Cancelled, 0);

            if (item == null)
                return (ActionResult.Success, 0);

            await UniTask.Yield(token);
        }

        if (destinationInvalid)
            return (ActionResult.Cancelled, 0);

        if (item == null || item.CurrentGridPosition != oldDestination)
            return (ActionResult.Success, 0);

        if (amount < 1) amount = 1;

        int takenAmount = Mathf.Min(amount, item.StackCount);

        if (takenAmount <= 0)
            return (ActionResult.Cancelled, 0);

        TakeItem(item, takenAmount);

        return (ActionResult.Success, takenAmount);
    }

    public async UniTask<(ActionResult, List<Item>)> TryDrop(
    ItemData item,
    ItemClass itemClass,
    int amount,
    CancellationToken token
    )
    {
        int remaining = amount;
        List<Item> droppedItems = null;

        while (remaining > 0)
        {
            token.ThrowIfCancellationRequested();

            // move to drop location
            //ActionResult moveResult =
            //    await MoveTo(targetPos, token);

            //if (moveResult != ActionResult.Success)
            //    return (moveResult, null);

            // try dropping
            (remaining, droppedItems) = DropItem(
                item,
                itemClass,
                remaining,
                currentGridPos + direction());

            // nothing dropped this loop
            if (remaining == amount)
                return (ActionResult.Cancelled, droppedItems);

            // update original amount for next loop check
            amount = remaining;

            // optional small delay
            await UniTask.Yield(token);
        }

        return (ActionResult.Success, droppedItems);
    }

    public async UniTask<ActionResult> DoProgressWork(
    Direction workDirection,
    CancellationToken token)
    {
        ChangeDirection(workDirection);

        progressBarInstance =
            Instantiate(progressBarPrefab,
            WorldCanvasUI.Instance.transform);

        progressBarInstance.Setup(transform, 0.5f);

        try
        {
            while (currentJob != null &&
                   !currentJob.removed &&
                   currentJob.ProgressCondition() &&
                   currentJob.currentProgress < currentJob.totalProgress)
            {
                token.ThrowIfCancellationRequested();

                currentJob.currentProgress += Time.deltaTime * 20;

                progressBarInstance.SetProgress(
                    currentJob.currentProgress /
                    currentJob.totalProgress);

                await UniTask.Yield(token);
            }
        }
        finally
        {
            if (progressBarInstance != null)
                Destroy(progressBarInstance.gameObject);
        }

        if (currentJob == null || currentJob.removed || !currentJob.ProgressCondition())
            return ActionResult.Cancelled;

        return ActionResult.Success;
    }
}