public partial class Pawn
{
    public void ControlPawn()
    {
        if (currentState == PawnState.Controlled) return;
        CancelPathfinding();
        CancelThink();
        CancelWear(true);
        ChangeDirection(Direction.South);
        ReturnJob();
        currentState = PawnState.Controlled;
        PathReset();
    }

    public void StopControlPawn()
    {
        if (currentState != PawnState.Controlled) return;
        CancelPathfinding();
        CancelWear(true);
        currentState = PawnState.Idle;
        PathReset();
    }
}
