using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class BuildingWorkable : Building
{
    [SerializeField] Transform baseWorkPosTf;
    [SerializeField] List<Transform> workPosTf;
    protected List<WorkPosition> workPos = new();
    [SerializeField] public Direction workDirection;

    public List<SkillRequirement> requiredSkills = new();
    protected override void Start()
    {
        base.Start();
        InitiateWorkPos(direction);
    }

    public WorkPosition[] GetAvailableWorkPos()
    {
        List<WorkPosition> availablePos = new();
        if (workPos.Count > 0)
        {
            foreach (WorkPosition pos in workPos)
            {
                if (!pos.occupied)
                    availablePos.Add(pos);
            }
        }
        return availablePos.ToArray();
    }

    public WorkPosition GetAvailableWorkPos(Pawn pawn)
    {
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
        //Debug.LogWarning("No available work position for " + pawn.ObjectName + " at " + objectName);
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


    public Action onDestroy;
    protected override void OnDestroy()
    {
        base.OnDestroy();
        onDestroy?.Invoke();
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