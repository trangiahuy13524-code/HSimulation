using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public abstract class JobBuilding : JobBase
{
    public override WorldObject refObject => workBuilding;
    public BuildingWorkable workBuilding;
    public WorkPosition reservedWorkPos;
    public override bool removed => workBuilding == null || externalRemoved;
    public override UniTask DoJob(
    Pawn worker,
    CancellationToken token)
    {
        return WorkToDo(worker, token);
    }

    async UniTask WorkToDo(
    Pawn worker,
    CancellationToken token)
    {

        if (reservedWorkPos == null)
        {
            worker.ReturnJob();
            return;
        }


        ActionResult result = await WorkRoutine(worker, reservedWorkPos, token);
        Debug.Log($"Work Routine Result: {result}");

        switch (result)
        {
            
            case ActionResult.Success:
                await FinishWork(worker, token);
                worker.RemoveJob();
                break;
            case ActionResult.Cancelled:
                OnJobCancelled(worker);
                break;
        }
    }



    protected abstract UniTask<ActionResult> WorkRoutine(
    Pawn worker,
    WorkPosition wP,
    CancellationToken token);

    public virtual void OnJobCancelled(Pawn worker)
    {
        Debug.Log("Job Cancelled");
        worker.ReturnJob();
    }

    public override void ReturnJob(Pawn worker)
    {
        if (reservedWorkPos != null)
        {
            reservedWorkPos.occupied = false;
            reservedWorkPos = null;
        }
    }

    protected override async UniTask FinishWork(Pawn worker, CancellationToken token)
    {
        if (reservedWorkPos != null)
        {
            reservedWorkPos.occupied = false;
            reservedWorkPos = null;
        }
        await base.FinishWork(worker, token);
    }
}