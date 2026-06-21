using Cysharp.Threading.Tasks;
using System.Threading;

public class JobResearch : JobBuilding
{
    public ResearchData researchData;

    protected override async UniTask<ActionResult> WorkRoutine(Pawn pawn, WorkPosition wP, CancellationToken token)
    {
        var moveResult = await pawn.MoveTo(wP.workPos, token);

        if (moveResult != ActionResult.Success)
            return moveResult;

        moveResult = await pawn.DoResearch(researchData, workBuilding.workDirection, token);

        return moveResult;
    }
}
