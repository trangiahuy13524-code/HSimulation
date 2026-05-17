using System.Collections.Generic;
using TMPro;
using UnityEngine;

public partial class Pawn : WorldObject
{
    [SerializeField] GameObject hightlight;

    [SerializeField] BodyData bodyData;
    [SerializeField] HeadData headData;
    [SerializeField] HairData hairData;
    //[SerializeField] FacialAnimator facial;
    [SerializeField] Direction oldDirection = Direction.South;
    public BodyData BodyData => bodyData;
    public HeadData HeadData => headData;
    public HairData HairData => hairData;
    //public FacialAnimator Facial => facial;

    public override Vector2 GetWorldPos()
    {
        return new Vector2(rb.position.x, rb.position.y - .5f);
    }

    public override string ObjectName
    {
        get
        {
            return objectName;
        }
        set
        {
            objectName = value;
            if (displayTextName) displayTextName.text = value;
        }
    }
    [SerializeField] TextMeshPro displayTextName;
    [SerializeField] Rigidbody2D rb;
    public Vector2 CurrentWorldPos => rb.position;
    [SerializeField] Vector2Int lastQueuePosCache;
    
    Queue<Vector2Int> paths;
    [SerializeField] Vector2Int oldDestination;
    [SerializeField] byte maxSearch = 10;
    public Queue<Vector2Int> Paths => paths;
    
    public void ChangeDirection(Direction dir)
    {
        if (dir == oldDirection) return;
        oldDirection = dir;
        if (bodyData) bodyData.SetDirection(dir);
        if (headData) headData.SetDirection(dir);
        if (hairData) hairData.SetDirection(dir);
        //if (facial) facial.SetDirection(dir);
    }
    void UpdateLayer()
    {
        if (bodyData) bodyData.UpdateLayer();
        if (headData) headData.UpdateLayer();
        if (hairData) hairData.UpdateLayer();
    }

    
    
    
    

    bool isRecalculating = false;
    public bool Move(byte speed)
    {
        if (paths.Count > 0)
        {
            Vector2Int nextPos = paths.Peek();
            if (!IsCurrentJobStillValid())
            {
                PathReset();
                return false;
            }
            if (isRecalculating)
            {
                if (paths.Count == 1 && nextPos == currentGridPos)
                {
                    PathReset();
                    return false;
                }
            }
            if (nextPos != currentGridPos && !world.IsPositionPathValid(nextPos))
            {
                if (onDuty)
                {
                    ReCalculatePath();
                }
                else
                {
                    PathReset();
                }
                return false;
            }
            Vector2Int delta = nextPos - currentGridPos;
            int x = delta.x;
            int y = delta.y;
            if (x != 0 && y != 0)
            {
                if (world.IsNotPassable(nextPos - new Vector2Int(x, 0)) || world.IsNotPassable(nextPos - new Vector2Int(0, y)))
                {
                    if (onDuty)
                    {
                        ReCalculatePath();
                    }
                    else
                    {
                        PathReset();
                    }
                    return false;
                }
            }

            Vector2 pos = rb.position;


            // reached tile
            if (pos == nextPos)
            {
                paths.Dequeue();
                currentGridPos = nextPos;
                nextPos = paths.Count > 0 ? paths.Peek() : currentGridPos;
                delta = nextPos - currentGridPos;


                if (delta != Vector2Int.zero)
                {
                    x = delta.x;
                    y = delta.y;
                    Direction dir;
                    if (x > 0) dir = Direction.East;
                    else if (x < 0) dir = Direction.West;
                    else if (y > 0) dir = Direction.North;
                    else dir = Direction.South;
                    ChangeDirection(dir);
                }
                world.ModifyPawnCountGrid(currentGridPos, true);
                world.ModifyPawnCountGrid(oldGridPos, false);
                oldGridPos = currentGridPos;
                UpdateLayer();
                if (paths.Count == 0)
                {
                    if (currentGridPos != oldDestination)
                    {
                        ReCalculatePath();
                        return false;
                    }
                    isRecalculating = false;
                    if (currentState == PawnState.Controlled)
                    {
                        ChangeDirection(Direction.South);
                    }
                    return true;
                }
            }
            float moveSpeed = Time.deltaTime * genome.speed * speed;
            if (onDuty) moveSpeed *= 2;
            Vector2 tempPos;
            if (nextPos == currentGridPos)
            {
                tempPos = Vector2.MoveTowards(pos, nextPos, moveSpeed * 3);
            }
            else
            {
                tempPos = Vector2.MoveTowards(pos, nextPos, moveSpeed);
            }

            rb.MovePosition(tempPos);

            return false;
        }
        else
        {
            
        }

        return true;
    }

    public void AddPathtoQueue(List<Vector2Int> pathList)
    {
        foreach (Vector2Int path in pathList)
        {
            if (lastQueuePosCache == path && paths.Count > 0) continue;
            paths.Enqueue(path);
            lastQueuePosCache = path;
        }
    }

    //void AddPathToQueue(Vector2Int path)
    //{
    //    if (paths.Count > 0)
    //    {
    //        if (lastQueuePosCache == path)
    //        {
    //            return;
    //        }
    //    }
    //    paths.Enqueue(path);
    //    lastQueuePosCache = path;
    //}

    Vector2Int GetRandomPosition()
    {
        int size = (world.WorldSize - 1)/2;

        int x = Random.Range(size - 10, size + 11);
        int y = Random.Range(size - 10, size + 11);

        return new Vector2Int(x, y);
    }

    public void MakePath(Vector2Int target)
    {
        var path = AStarPathfinder.FindPath(currentGridPos, target, maxSearch);

        if (path != null)
        {
            paths.Clear();
            oldDestination = target;
            AddPathtoQueue(path);
        }
        else
        {
            Debug.Log("NoPath! nullxx");
        }
    }

    public void MakePathContinuous(Vector2Int target)
    {
        List<Vector2Int> path;
        if (paths.Count > 0)
        {
            Vector2Int next = paths.Peek();
            path = AStarPathfinder.FindPath(next, target, maxSearch, currentGridPos);
        }
        else
        {
            path = AStarPathfinder.FindPath(currentGridPos, target, maxSearch);
        }

        

        if (path != null)
        {
            paths.Clear();
            oldDestination = target;
            AddPathtoQueue(path);
        }
        else
        {
            Debug.Log("NoPath! nullxx");
        }
    }

    void ReCalculatePath()
    {
        var path = AStarPathfinder.FindPath(currentGridPos, oldDestination, maxSearch);
        if (path != null)
        {
            isRecalculating = true;
            paths.Clear();
            AddPathtoQueue(path);
        }
        else
        {
            PathReset();
        }
    }

    public void PathReset()
    {
        paths.Clear();
        paths.Enqueue(currentGridPos);
        oldDestination = currentGridPos;
        destinationInvalid = true;
        reachDestination = true;
        isRecalculating = false;
    }

    //public void PathResetContinuous()
    //{
    //    if (paths.Count > 0)
    //    {
    //        Vector2Int next = paths.Peek();
    //        paths.Clear();
    //        paths.Enqueue(next);
    //        oldDestination = next;
    //        canReachWork = false;
    //        isRecalculating = false;
    //    }
    //    else
    //    {
    //        PathReset();
    //    }
    //}
}
