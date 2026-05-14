using UnityEngine;

public partial class Pawn : WorldObject
{
    public bool isControlled { get; private set; } = false;

    public void ControlPawn()
    {
        if (isControlled) return;
        isControlled = true;
        ChangeDirection(Direction.South);
        ReturnJob();
        PathReset();
    }

    public void StopControlPawn()
    {
        if (!isControlled) return;
        isControlled = false;
        PathReset();
    }
}
