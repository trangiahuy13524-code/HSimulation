using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pawn : BaseObject
{
    [SerializeField] BodyData bodyData;
    [SerializeField] HeadData headData;
    [SerializeField] HairData hairData;
    [SerializeField] Direction oldDirection = Direction.South;
    public BodyData BodyData => bodyData;
    public HeadData HeadData => headData;
    public HairData HairData => hairData;


    [SerializeField] Rigidbody2D rb;
    [SerializeField] Vector2Int lastQueuePosCache;
    [SerializeField] float idleTime = 2f;
    [SerializeField] float currentIdleTime = 0f;
    Queue<Vector2Int> paths;
    [SerializeField] Vector2Int oldDestination;
    [SerializeField] Vector2Int oldGridPos;
    public GeneticData geneticData;

    struct DirectionData
    {
        public Vector2Int vector;
        public Direction direction;
        public DirectionData(Vector2Int vector, Direction direction)
        {
            this.vector = vector;
            this.direction = direction;
        }
    }

    public void ChangeDirection(Direction dir)
    {
        if (dir == oldDirection) return;
        oldDirection = dir;
        if (bodyData) bodyData.SetDirection(dir);
        if (headData) headData.SetDirection(dir);
        if (hairData) hairData.SetDirection(dir);
    }




    protected override void Start()
    {
        base.Start();
        paths = new Queue<Vector2Int>();
        oldGridPos = currentGridPos;
        world.ModifyPawnCountGrid(currentGridPos, true);
    }
    private void Update()
    {
        bool donePathing = Move();
        if (donePathing)
        {
            if (currentIdleTime < idleTime)
            {
                currentIdleTime += Time.deltaTime;
            }
            else
            {
                currentIdleTime = 0f;
                GetRandomPosition();
            }
        }
        
    }

    public bool Move()
    {
        if (paths.Count == 0)
            return true;

        world.UnregisterObject(currentGridPos, this);
        Vector2Int nextPos = paths.Peek();

        if (world.IsPositionOccupied(lastQueuePosCache))
        {
            paths.Clear();
            paths.Enqueue(currentGridPos);
            return false;
        }
        if (!world.IsPositionValid(nextPos))
        {
            ReCalculatePath();
            return false;
        }
        Vector2Int delta = nextPos - currentGridPos;
        int x = delta.x;
        int y = delta.y;
        if (x != 0 && y != 0)
        {
            if (!world.IsPositionValid(nextPos - new Vector2Int(x, 0)) || !world.IsPositionValid(nextPos - new Vector2Int(0, y)))
            {
                ReCalculatePath();
                return false;
            }
        }

        Vector2 pos = rb.position;

        // Always face target while moving
        

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
            if (paths.Count == 0)
            {
                world.RegisterObject(this, currentGridPos);
                return true;
            }
        }
        float moveSpeed = Time.deltaTime;
        Vector2 tempPos;
        if (nextPos == currentGridPos)
        {
            tempPos = Vector2.MoveTowards(pos, nextPos, moveSpeed * 2);
        }
        else
        {
            tempPos = Vector2.MoveTowards(pos, nextPos, moveSpeed);
        }
        

        rb.MovePosition(tempPos);

        return false;
    }

    public void AddPathtoQueue(List<Vector2Int> pathList)
    {
        foreach (Vector2Int path in pathList)
        {
            AddPathToQueue(path);
        }
    }

    public void AddPathToQueue(Vector2Int path)
    {
        if (paths.Count > 0)
        {
            if (lastQueuePosCache == path)
            {
                return;
            }
        }
        paths.Enqueue(path);
        lastQueuePosCache = path;
    }

    public void GetRandomPosition()
    {
        int size = World.Instance.WorldSize - 1;

        int x = Random.Range(size/4, size*3/4);
        int y = Random.Range(size/4, size*3/4);

        Vector2Int targetPos = new Vector2Int(x, y);

        var path = AStarPathfinder.FindPath(currentGridPos, targetPos);

        if (path != null)
        {
            oldDestination = targetPos;
            AddPathtoQueue(path);
        }
    }

    public void ReCalculatePath()
    {
        if (paths.Count == 0) return;
        Vector2Int targetPos = oldDestination;
        if (!world.IsPositionValid(targetPos))
        {
            PathReset();
            return;
        }
        var path = AStarPathfinder.FindPath(currentGridPos, targetPos);
        if (path != null)
        {
            paths.Clear();
            AddPathtoQueue(path);
        }
        else
        {
            PathReset();
        }
    }

    private void PathReset()
    {
        paths.Clear();
        paths.Enqueue(currentGridPos);
    }
}
