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
    Pawn pawn,
    CancellationToken token)
    {

        if (reservedWorkPos == null)
        {
            pawn.ReturnJob();
            return;
        }


        ActionResult result = await WorkRoutine(pawn, reservedWorkPos, token);
        Debug.Log($"Work Routine Result: {result}");

        switch (result)
        {
            case ActionResult.Success:
                await FinishWork(pawn, token);
                pawn.RemoveJob();
                break;
            case ActionResult.Cancelled:
                OnJobCancelled(pawn);
                break;
        }
    }



    protected abstract UniTask<ActionResult> WorkRoutine(
    Pawn pawn,
    WorkPosition wP,
    CancellationToken token);

    public virtual void OnJobCancelled(Pawn pawn)
    {
        Debug.Log("Job Cancelled");
        pawn.ReturnJob();
    }

    public override void ReturnJob()
    {
        if (reservedWorkPos != null)
        {
            reservedWorkPos.occupied = false;
            reservedWorkPos = null;
        }
    }
}