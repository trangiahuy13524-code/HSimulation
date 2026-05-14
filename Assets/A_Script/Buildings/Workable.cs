using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Workable : Building
{
    [SerializeField] Transform baseWorkPosTf;
    [SerializeField] List<Transform> workPosTf;
    protected List<WorkPosition> workPos = new();
    [SerializeField] public Direction workDirection;
    
    protected override void Start()
    {
        base.Start();
        InitiateWorkPos(direction);
    }
    public virtual void DoTask(Pawn pawn, string taskName)
    {
        return;
    }
    public WorkPosition GetAvailableWorkPos(Pawn pawn)
    {
        Debug.Log(workPos.Count);
        if (workPos.Count > 0)
        {
            foreach(WorkPosition pos in workPos)
            {
                if (!pos.occupied)
                {
                    if (!world.IsPositionPathValid(pos.workPos) && pos.workPos != pawn.CurrentGridPosition)
                        continue;
                    return pos;
                }
            }
        }
        return null;
    }

    public void InitiateWorkPos(Direction direction)
    {
        
        switch (direction)
        {
            case Direction.North:
                baseWorkPosTf.localEulerAngles = new Vector3(180, 0, 0);
                workDirection = Direction.South;
                break;
            case Direction.South:
                baseWorkPosTf.localEulerAngles = new Vector3(0, 0, 0);
                workDirection = Direction.North;
                break;
            case Direction.West:
                baseWorkPosTf.localEulerAngles = new Vector3(0, 180, 90);
                workDirection = Direction.East;
                break;
            default:
                baseWorkPosTf.localEulerAngles = new Vector3(0, 0, 90);
                workDirection = Direction.West;
                break;
        }

        foreach (Transform t in workPosTf)
        {
            WorkPosition wP = new WorkPosition(WorldUtility.WorldPosToGridPos(t.position));
            workPos.Add(wP);
        }
    }
    public virtual void PerformWork(Pawn pawn)
    {
        StartCoroutine(WorkToDo(pawn));
    }

    protected abstract IEnumerator WorkRoutine(Pawn pawn, WorkPosition wP);

    IEnumerator WorkToDo(Pawn pawn)
    {
        WorkPosition wP = GetAvailableWorkPos(pawn);
        if (wP == null)
        {
            pawn.ReturnJob();
            yield break;
        }

        wP.occupied = true;
        pawn.SetWorkPosition(wP);

        yield return WorkRoutine(pawn, wP);
        if (pawn.lastWorkResult == WorkResult.Success)
            yield return pawn.FinishWork();

        // Decide AFTER coroutine finishes
        switch (pawn.lastWorkResult)
        {
            case WorkResult.Success:
                pawn.RemoveJob();
                break;

            case WorkResult.Failed:
            case WorkResult.Cancelled:
                pawn.ReturnJob();
                break;
        }
    }
}

public class WorkPosition
{
    public Vector2Int workPos;
    public bool occupied = false;

    public WorkPosition(Vector2Int pos)
    {
        workPos = pos;
    }
}