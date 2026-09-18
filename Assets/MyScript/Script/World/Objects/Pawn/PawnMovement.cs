using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public partial class Pawn
{
    private const int RANDOM_WANDER_RADIUS = 10;
    private const int RANDOM_RANGE_MAX_OFFSET = 1;
    private const int RECALCULATE_PATH_THRESHOLD = 2;
    private const float ON_DUTY_SPEED_MULTIPLIER = 2f;
    private const float CURRENT_TILE_SPEED_MULTIPLIER = 3f;

    [Header("Pawn Movement")]
    [SerializeField] Vector2Int lastQueuePosCache;
    [SerializeField] Vector2Int oldDestination;
    [SerializeField] Vector2Int itemDestination;
    [SerializeField] byte maxSearch = 10;

    private Queue<Vector2Int> paths;
    private CancellationTokenSource cts = new CancellationTokenSource();
    private bool calculatingPath;
    private bool isRecalculating;
    private bool recalculateTriggered;
    private bool recalculateTaskRunning;

    public Queue<Vector2Int> Paths => paths;
    public bool withoutLast = false;

    public bool Move(byte speed, WorldThreadSafe worldTS)
    {
        if (recalculateTaskRunning || calculatingPath) return false;
        if (paths.Count == 0) return true;

        Vector2Int nextPos = paths.Peek();
        if (!IsCurrentJobStillValid() || !IsRecalculatedPathValid())
        {
            PathReset();
            return false;
        }

        if (!CanMoveTo(nextPos))
        {
            HandleBlockedPath(nextPos, worldTS);
            return false;
        }

        if (rb.position == nextPos)
        {
            ReachPathNode(nextPos);
            nextPos = paths.Count > 0 ? paths.Peek() : currentGridPos;
            ChangeDirectionFromDelta(nextPos - currentGridPos);

            if (ShouldRecalculatePath())
            {
                ReCalculatePath(worldTS).Forget();
                return false;
            }

            if (paths.Count == 0)
            {
                FinishMovement();
                return true;
            }
        }

        MoveTowards(nextPos, speed);
        return false;
    }

    private bool IsRecalculatedPathValid()
    {
        if (!isRecalculating) return true;

        return withoutLast
            ? lastQueuePosCache != currentGridPos || currentGridPos == itemDestination
            : lastQueuePosCache != currentGridPos;
    }

    private bool CanMoveTo(Vector2Int nextPos)
    {
        if (nextPos == currentGridPos) return true;
        if (!world.IsPositionPathValid(nextPos)) return false;

        Vector2Int delta = nextPos - currentGridPos;
        if (delta.x == 0 || delta.y == 0) return true;

        Vector2Int horizontalNeighbour = nextPos - new Vector2Int(delta.x, 0);
        Vector2Int verticalNeighbour = nextPos - new Vector2Int(0, delta.y);
        return !world.IsNotPassable(horizontalNeighbour) && !world.IsNotPassable(verticalNeighbour);
    }

    private void HandleBlockedPath(Vector2Int nextPos, WorldThreadSafe worldTS)
    {
        if (!onDuty)
        {
            PathReset();
            return;
        }

        Vector2Int destination = withoutLast ? itemDestination : oldDestination;
        if (nextPos == destination)
        {
            PathReset();
        }

        ReCalculatePath(worldTS).Forget();
    }

    private void ReachPathNode(Vector2Int reachedPosition)
    {
        paths.Dequeue();
        currentGridPos = reachedPosition;
        world.ModifyPawnCountGrid(currentGridPos, true);
        world.ModifyPawnCountGrid(oldGridPos, false);
        oldGridPos = currentGridPos;
        UpdateLayer();
    }

    private void ChangeDirectionFromDelta(Vector2Int delta)
    {
        if (delta == Vector2Int.zero) return;

        Direction direction;
        if (delta.x > 0) direction = Direction.East;
        else if (delta.x < 0) direction = Direction.West;
        else if (delta.y > 0) direction = Direction.North;
        else direction = Direction.South;

        ChangeDirection(direction);
    }

    private bool ShouldRecalculatePath()
    {
        if (recalculateTriggered || paths.Count > RECALCULATE_PATH_THRESHOLD) return false;

        Vector2Int destination = withoutLast ? itemDestination : oldDestination;
        return lastQueuePosCache != destination;
    }

    private void FinishMovement()
    {
        isRecalculating = false;
        if (currentState == PawnState.Controlled)
        {
            ChangeDirection(Direction.South);
        }
    }

    private void MoveTowards(Vector2Int nextPos, byte speed)
    {
        float moveSpeed = Time.deltaTime * genome.speed * speed;
        if (onDuty)
        {
            moveSpeed *= ON_DUTY_SPEED_MULTIPLIER;
        }

        if (nextPos == currentGridPos)
        {
            moveSpeed *= CURRENT_TILE_SPEED_MULTIPLIER;
        }

        rb.MovePosition(Vector2.MoveTowards(rb.position, nextPos, moveSpeed));
    }

    public void AddPathtoQueue(List<Vector2Int> pathList)
    {
        foreach (Vector2Int path in pathList)
        {
            EnqueueUniquePath(path);
        }
    }

    public void AddPathtoQueueWithoutLast(List<Vector2Int> pathList)
    {
        if (pathList.Count <= 1)
        {
            Vector2Int path = pathList[0];
            itemDestination = path;
            lastQueuePosCache = path;
            paths.Enqueue(path);
            return;
        }

        for (int i = 0; i < pathList.Count; i++)
        {
            Vector2Int path = pathList[i];
            if (i == pathList.Count - 1 && path == oldDestination)
            {
                itemDestination = pathList[i - 1];
                break;
            }

            EnqueueUniquePath(path);
        }
    }

    private void EnqueueUniquePath(Vector2Int path)
    {
        if (lastQueuePosCache == path && paths.Count > 0) return;

        paths.Enqueue(path);
        lastQueuePosCache = path;
    }

    private Vector2Int GetRandomPosition()
    {
        int center = (world.WorldSize - 1) / 2;
        int maxExclusive = center + RANDOM_WANDER_RADIUS + RANDOM_RANGE_MAX_OFFSET;
        int x = Random.Range(center - RANDOM_WANDER_RADIUS, maxExclusive);
        int y = Random.Range(center - RANDOM_WANDER_RADIUS, maxExclusive);
        return new Vector2Int(x, y);
    }

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
        List<Vector2Int> path = await UniTask.RunOnThreadPool(
            () => AStarPathfinder.FindPath(currentGridPos, target, maxSearch, worldTS),
            cancellationToken: cts.Token);

        if (path != null)
        {
            ReplacePath(path, target, false);
        }
        else
        {
            Debug.Log("No path");
        }

        calculatingPath = false;
    }

    public async UniTask MakePathWithoutLast(Vector2Int target, WorldThreadSafe worldTS)
    {
        calculatingPath = true;
        List<Vector2Int> path = await UniTask.RunOnThreadPool(
            () => AStarPathfinder.FindPath(currentGridPos, target, maxSearch, worldTS),
            cancellationToken: cts.Token);

        if (path != null)
        {
            ReplacePath(path, target, true);
        }
        else
        {
            Debug.Log("NoPath! nullxx");
        }

        calculatingPath = false;
    }

    public async UniTask MakePathContinuous(Vector2Int target, WorldThreadSafe worldTS)
    {
        calculatingPath = true;
        List<Vector2Int> path;

        if (paths.Count > 0)
        {
            Vector2Int next = paths.Peek();
            path = await UniTask.RunOnThreadPool(
                () => AStarPathfinder.FindPath(next, target, maxSearch, currentGridPos, worldTS),
                cancellationToken: cts.Token);
        }
        else
        {
            path = await UniTask.RunOnThreadPool(
                () => AStarPathfinder.FindPath(currentGridPos, target, maxSearch, worldTS),
                cancellationToken: cts.Token);
        }

        if (path != null)
        {
            ReplacePath(path, target, false);
        }
        else
        {
            Debug.Log("NoPath! nullxx");
        }

        calculatingPath = false;
    }

    private void ReplacePath(List<Vector2Int> path, Vector2Int target, bool excludeLast)
    {
        paths.Clear();
        withoutLast = excludeLast;
        oldDestination = target;

        if (excludeLast)
        {
            AddPathtoQueueWithoutLast(path);
        }
        else
        {
            AddPathtoQueue(path);
        }
    }

    private async UniTask ReCalculatePath(WorldThreadSafe worldTS)
    {
        recalculateTriggered = true;
        recalculateTaskRunning = true;
        List<Vector2Int> path;

        if (paths.Count > 0)
        {
            Vector2Int next = paths.Peek();
            path = await UniTask.RunOnThreadPool(
                () => AStarPathfinder.FindPath(next, oldDestination, maxSearch, currentGridPos, worldTS),
                cancellationToken: cts.Token);
        }
        else
        {
            path = await UniTask.RunOnThreadPool(
                () => AStarPathfinder.FindPath(currentGridPos, oldDestination, maxSearch, worldTS),
                cancellationToken: cts.Token);
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
