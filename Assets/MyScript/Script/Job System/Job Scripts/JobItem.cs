using Cysharp.Threading.Tasks;
using System.Threading;

public abstract class JobItem : JobBase
{
    public override WorldObject refObject => refItem;
    public Item refItem;
    public override bool removed => refItem == null || externalRemoved;

    public override async UniTask DoJob(Pawn worker, CancellationToken token)
    {
        await ExecuteWork(worker, token, () => WorkRoutine(worker, token));
    }

    protected abstract UniTask WorkRoutine(Pawn worker, CancellationToken token);

    public override void ReturnJob(Pawn worker)
    {
        if (refItem != null)
        {
            refItem.reservingObject = null;
        }
    }
}
