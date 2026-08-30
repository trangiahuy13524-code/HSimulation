using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public partial class Pawn : WorldObjectDynamic
{
    [Header("Pawn Movement")]

    [SerializeField] SpriteBody bodyData;
    [SerializeField] SpriteHead headData;
    [SerializeField] SpriteHair hairData;
    //[SerializeField] FacialAnimator facial;
    [SerializeField] Direction oldDirection = Direction.South;
    public Direction CurrentDirection => oldDirection;
    public SpriteBody BodyData => bodyData;
    public SpriteHead HeadData => headData;
    public SpriteHair HairData => hairData;
    //public FacialAnimator Facial => facial;

    public override Vector2 GetWorldPos()
    {
        return new Vector2(rb.position.x, rb.position.y - .5f);
    }
    [SerializeField] TextMeshPro displayTextName;
    [SerializeField] Rigidbody2D rb;
    public Vector2 CurrentWorldPos => rb.position;
    [SerializeField] Vector2Int lastQueuePosCache;

    Queue<Vector2Int> paths;
    [SerializeField] Vector2Int oldDestination;
    [SerializeField] Vector2Int itemDestination;
    [SerializeField] byte maxSearch = 10;
    public Queue<Vector2Int> Paths => paths;

    public Vector2Int direction()
    {
        switch (oldDirection)
        {
            case Direction.North: return Vector2Int.up;
            case Direction.South: return Vector2Int.down;
            case Direction.East: return Vector2Int.right;
            case Direction.West: return Vector2Int.left;
            default: return Vector2Int.zero;
        }
    }

    public void ChangeDirection(Direction dir)
    {
        if (dir == oldDirection) return;
        oldDirection = dir;
        if (bodyData) bodyData.SetDirection(dir);
        if (headData) headData.SetDirection(dir);
        if (hairData) hairData.SetDirection(dir);
        ChangeAttireDirection(dir);
        //if (facial) facial.SetDirection(dir);
    }
    public override void UpdateLayer()
    {
        if (bodyData) bodyData.UpdateLayer();
        if (headData) headData.UpdateLayer();
        if (hairData) hairData.UpdateLayer();
        UpdateAttireLayer();
    }

    bool isRecalculating = false;
    public bool Move(byte speed, WorldThreadSafe worldTS)
    {
        if (recalculateTaskRunning || calculatingPath) return false;

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
                if (withoutLast)
                {
                    if (lastQueuePosCache == currentGridPos && currentGridPos != itemDestination)
                    {
                        PathReset();
                        return false;
                    }
                }
                else
                {
                    if (lastQueuePosCache == currentGridPos)
                    {
                        PathReset();
                        return false;
                    }
                }
                
            }
            if (nextPos != currentGridPos && !world.IsPositionPathValid(nextPos))
            {
                
                if (onDuty)
                {
                    if (withoutLast)
                    {
                        if (nextPos == itemDestination)
                        {
                            PathReset();
                        }
                    }
                    else
                    {
                        if (nextPos == oldDestination)
                        {
                            PathReset();
                        }
                    }
                    ReCalculatePath(worldTS).Forget();
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
                        if (withoutLast)
                        {
                            if (nextPos == itemDestination)
                            {
                                PathReset();
                            }
                        }
                        else
                        {
                            if (nextPos == oldDestination)
                            {
                                PathReset();
                            }
                        }
                        ReCalculatePath(worldTS).Forget();
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
                if (!recalculateTriggered && paths.Count <= 2)
                {
                    if (withoutLast)
                    {
                        if (lastQueuePosCache != itemDestination)
                        {
                            ReCalculatePath(worldTS).Forget();
                            return false;
                        }
                    }
                    else
                    {
                        if (lastQueuePosCache != oldDestination)
                        {
                            ReCalculatePath(worldTS).Forget();
                            return false;
                        }
                    }
                }

                if (paths.Count == 0)
                {
                    //if (withoutLast)
                    //{
                    //    if (currentGridPos != itemDestination)
                    //    {
                    //        ReCalculatePath().Forget();
                    //        return false;
                    //    }
                    //}
                    //else
                    //{
                    //    if (currentGridPos != oldDestination)
                    //    {
                    //        ReCalculatePath().Forget();
                    //        return false;
                    //    }
                    //}

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
    public void AddPathtoQueueWithoutLast(List<Vector2Int> pathList)
    {
        if (pathList.Count > 1)
        {
            for (int i = 0; i < pathList.Count; i++) // skip last
            {
                Vector2Int path = pathList[i];

                if (i == pathList.Count - 1)
                {
                    if (path == oldDestination)
                    {
                        itemDestination = pathList[i - 1];
                        break;
                    }
                }

                if (lastQueuePosCache == path && paths.Count > 0)
                    continue;

                paths.Enqueue(path);
                lastQueuePosCache = path;
            }
        }
        else
        {
            Vector2Int path = pathList[0];
            itemDestination = path;
            lastQueuePosCache = path;
            paths.Enqueue(path);
        }
    }

    Vector2Int GetRandomPosition()
    {
        int size = (world.WorldSize - 1)/2;

        int x = Random.Range(size - 10, size + 11);
        int y = Random.Range(size - 10, size + 11);

        return new Vector2Int(x, y);
    }

    WorldThreadSafe worldTS;
    CancellationTokenSource cts = new CancellationTokenSource();
    bool calculatingPath = false;
    public void CancelPathfinding()
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        calculatingPath = false;
        recalculateTaskRunning = false;
    }

    public async UniTask MakePath(Vector2Int target, WorldThreadSafe worldTS)
    {
        calculatingPath = true;
        List<Vector2Int> path =
            await UniTask.RunOnThreadPool(() =>
            {
                return AStarPathfinder.FindPath(
                    currentGridPos,
                    target,
                    maxSearch,
                    worldTS);
            },
            cancellationToken: cts.Token);

        if (path != null)
        {
            paths.Clear();
            withoutLast = false;
            oldDestination = target;
            AddPathtoQueue(path);
        }
        else
        {
            Debug.Log("No path");
        }
        calculatingPath = false;
        return;
    }
    public bool withoutLast = false;
    public async UniTask MakePathWithoutLast(Vector2Int target, WorldThreadSafe worldTS)
    {
        calculatingPath = true;
        var path = await UniTask.RunOnThreadPool(() =>
        {
            return AStarPathfinder.FindPath(currentGridPos, target, maxSearch, worldTS);
        }, cancellationToken: cts.Token);

        if (path != null)
        {
            paths.Clear();
            withoutLast = true;
            oldDestination = target;
            AddPathtoQueueWithoutLast(path);
        }
        else
        {
            Debug.Log("NoPath! nullxx");
        }
        calculatingPath = false;
        return;
    }

    public async UniTask MakePathContinuous(Vector2Int target, WorldThreadSafe worldTS)
    {
        calculatingPath = true;
        List<Vector2Int> path;

        if (paths.Count > 0)
        {
            Vector2Int next = paths.Peek();
            path = await UniTask.RunOnThreadPool(() =>
            {
                return AStarPathfinder.FindPath(next, target, maxSearch, currentGridPos, worldTS);
            }, cancellationToken: cts.Token);
        }
        else
        {
            path = await UniTask.RunOnThreadPool(() =>
            {
                return AStarPathfinder.FindPath(currentGridPos, target, maxSearch, worldTS);
            }, cancellationToken: cts.Token);
        }



        if (path != null)
        {
            paths.Clear();
            withoutLast = false;
            oldDestination = target;
            AddPathtoQueue(path);
        }
        else
        {
            Debug.Log("NoPath! nullxx");
        }
        calculatingPath = false;
        return;
    }

    bool recalculateTriggered = false;
    bool recalculateTaskRunning = false;
    async UniTask ReCalculatePath(WorldThreadSafe worldTS)
    {
        recalculateTriggered = true;
        recalculateTaskRunning = true;
        List<Vector2Int> path;

        if (paths.Count > 0)
        {
            Vector2Int next = paths.Peek();
            path = await UniTask.RunOnThreadPool(() =>
            {
                return AStarPathfinder.FindPath(next, oldDestination, maxSearch, currentGridPos, worldTS);
            }, cancellationToken: cts.Token);
        }
        else
        {
            path = await UniTask.RunOnThreadPool(() =>
            {
                return AStarPathfinder.FindPath(currentGridPos, oldDestination, maxSearch, worldTS);
            }, cancellationToken: cts.Token);
        }

        if (path != null)
        {
            isRecalculating = true;
            paths.Clear();
            if (withoutLast)
            {
                AddPathtoQueueWithoutLast(path);
            }
            else
            {
                AddPathtoQueue(path);
            }
        }
        else
        {
            PathReset();
        }
        recalculateTaskRunning = false;
        recalculateTriggered = false;
    }

    public void PathReset()
    {
        CancelPathfinding();
        paths.Clear();
        paths.Enqueue(currentGridPos);
        oldDestination = currentGridPos;
        destinationInvalid = true;
        reachDestination = true;
        isRecalculating = false;
        withoutLast = false;
    }

    
}
