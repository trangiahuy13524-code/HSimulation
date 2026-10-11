using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class JobWearAttire : JobItem
{
    private const int WORN_ITEM_AMOUNT = 1;

    protected override async UniTask WorkRoutine(Pawn worker, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        using (token.Register(() => worker.CancelWear()))
        {
            bool worn = await worker.MoveToAndWear(refItem);
            token.ThrowIfCancellationRequested();

            if (!worn)
            {
                throw new OperationCanceledException(token);
            }

            refItem.ReduceStack(WORN_ITEM_AMOUNT);
        }
    }
}
