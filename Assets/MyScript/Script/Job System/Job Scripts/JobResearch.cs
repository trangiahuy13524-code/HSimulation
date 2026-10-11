using Cysharp.Threading.Tasks;
using System.Threading;

public class JobResearch : JobBuilding
{
    public DataJobResearch researchData;

    protected override async UniTask WorkRoutine(
        Pawn pawn,
        WorkPosition wP,
        CancellationToken token)
    {
        await pawn.MoveTo(wP.workPos, token);
        await pawn.DoResearch(researchData, workBuilding.workDirection, token);
    }
}
