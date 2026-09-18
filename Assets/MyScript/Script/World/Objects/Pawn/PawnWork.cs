using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public partial class Pawn
{
    private const float WORK_PROGRESS_PER_SECOND = 20f;
    private const float PROGRESS_BAR_VERTICAL_OFFSET = 0.5f;

    [Header("Pawn Work")]
    private Dictionary<DataSkill, byte> pawnSkills = new();
    [SerializeField] ProgressBar progressBarPrefab;
    private ProgressBar progressBarInstance;
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
        if (currentJob != null) return false;

        JobBase job = await jobManager.GetJob(this, token);
        if (job == null) return false;

        currentJob = job;
        return true;
    }

    public void RemoveJob()
    {
        currentState = PawnState.Idle;
        jobManager.RemoveJob(currentJob);
        currentJob = null;
    }

    public void ReturnJob()
    {
        if (currentJob == null) return;

        currentState = PawnState.Idle;
        jobManager.ReturnJob(currentJob, this);
        currentJob = null;
    }

    private bool IsCurrentJobStillValid()
    {
        if (currentJob == null || !currentJob.removed) return true;

        RemoveJob();
        return false;
    }

    public async UniTask<ActionResult> MoveTo(Vector2Int targetPos, CancellationToken token)
    {
        reachDestination = false;
        destinationInvalid = false;
        await MakePath(targetPos, PawnManager.Instance.GetWTS());

        while (!reachDestination)
        {
            if (currentJob == null || currentJob.removed) return ActionResult.Cancelled;
            await UniTask.Yield(token);
        }

        return destinationInvalid ? ActionResult.Cancelled : ActionResult.Success;
    }

    public async UniTask<(ActionResult, int)> MoveToAndPickUp(Item item, int amount, CancellationToken token)
    {
        reachDestination = false;
        destinationInvalid = false;

        item.reservingObject = this;

        await MakePathWithoutLast(item.CurrentGridPosition, PawnManager.Instance.GetWTS());

        await UniTask.Yield(token);
        while (!reachDestination)
        {
            if (currentJob == null || currentJob.removed)
            {
                item.reservingObject = null;
                return (ActionResult.Cancelled, 0);
            }

            if (item == null)
            {
                return (ActionResult.Success, 0);
            }

            await UniTask.Yield(token);
        }

        if (destinationInvalid)
        {
            item.reservingObject = null;
            return (ActionResult.Cancelled, 0);
        }

        if (item == null || item.CurrentGridPosition != oldDestination)
            return (ActionResult.Success, 0);

        if (amount < 1) amount = 1;

        int takenAmount = Mathf.Min(amount, item.StackCount);

        if (takenAmount <= 0)
        {
            item.reservingObject = null;
            return (ActionResult.Cancelled, 0);
        }

        takenAmount = HoldItem(item, takenAmount);
        await UniTask.Yield(token);
        return (ActionResult.Success, takenAmount);
    }

    public async UniTask<ActionResult> DoProgressWork(
    Direction workDirection,
    CancellationToken token)
    {
        ChangeDirection(workDirection);
        CreateProgressBar();

        try
        {
            while (currentJob != null &&
                   !currentJob.removed &&
                   currentJob.currentProgress < currentJob.totalProgress)
            {
                token.ThrowIfCancellationRequested();

                currentJob.currentProgress += Time.deltaTime * WORK_PROGRESS_PER_SECOND;
                progressBarInstance.SetProgress(currentJob.currentProgress / currentJob.totalProgress);

                await UniTask.Yield(token);
            }
        }
        finally
        {
            DestroyProgressBar();
        }

        if (currentJob == null || currentJob.removed)
            return ActionResult.Cancelled;

        return ActionResult.Success;
    }

    public async UniTask<ActionResult> DoResearch(
    DataJobResearch researchData,
    Direction workDirection,
    CancellationToken token)
    {
        ChangeDirection(workDirection);
        var state = researchManager.GetState(researchData);
        CreateProgressBar();

        try
        {
            while (currentJob != null &&
                   !currentJob.removed &&
                   !state.completed)
            {
                token.ThrowIfCancellationRequested();

                researchManager.AddProgress(researchData, Time.deltaTime * WORK_PROGRESS_PER_SECOND);
                progressBarInstance.SetProgress(state.progress / researchData.totalProgress);

                await UniTask.Yield(token);
            }
        }
        finally
        {
            DestroyProgressBar();
        }

        if (currentJob == null || currentJob.removed)
            return ActionResult.Cancelled;

        return ActionResult.Success;
    }

    private void CreateProgressBar()
    {
        progressBarInstance = Instantiate(progressBarPrefab, WorldCanvasUI.Instance.transform);
        progressBarInstance.Setup(transform, PROGRESS_BAR_VERTICAL_OFFSET);
    }

    private void DestroyProgressBar()
    {
        if (progressBarInstance != null)
        {
            Destroy(progressBarInstance.gameObject);
        }
    }
}
