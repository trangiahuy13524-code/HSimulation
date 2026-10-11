using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public abstract class JobBuilding : JobBase
{
    public override WorldObject refObject => workBuilding;
    public BuildingWorkable workBuilding;
    public WorkPosition reservedWorkPos;
    public override bool removed => workBuilding == null || externalRemoved;

    public override async UniTask DoJob(Pawn worker, CancellationToken token)
    {
        await ExecuteWork(worker, token, async () =>
        {
            if (reservedWorkPos == null)
            {
                throw new OperationCanceledException(token);
            }

            await WorkRoutine(worker, reservedWorkPos, token);
        });
    }

    protected abstract UniTask WorkRoutine(
        Pawn worker,
        WorkPosition wP,
        CancellationToken token);

    public override void OnJobCancelled(Pawn worker)
    {
        base.OnJobCancelled(worker);
    }

    public override void ReturnJob(Pawn worker)
    {
        if (reservedWorkPos != null)
        {
            reservedWorkPos.occupied = false;
            reservedWorkPos = null;
        }
    }
}
