using UnityEngine;

public partial class Pawn : WorldObject
{

    public void ControlPawn()
    {
        if (currentState == PawnState.Controlled) return;
        ChangeDirection(Direction.South);
        ReturnJob();
        currentState = PawnState.Controlled;
        PathReset();
    }

    public void StopControlPawn()
    {
        if (currentState != PawnState.Controlled) return;
        currentState = PawnState.Idle;
        PathReset();
    }
}
