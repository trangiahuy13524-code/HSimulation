using UnityEngine;

public partial class Pawn
{

    public void ControlPawn()
    {
        if (currentState == PawnState.Controlled) return;
        CancelPathfinding();
        CancelThink();
        ChangeDirection(Direction.South);
        ReturnJob();
        currentState = PawnState.Controlled;
        PathReset();
    }

    public void StopControlPawn()
    {
        if (currentState != PawnState.Controlled) return;
        CancelPathfinding();
        currentState = PawnState.Idle;
        PathReset();

    }
}
