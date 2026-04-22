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
        oldGridPos = currentGridPos;
        paths = new Queue<Vector2Int>();
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

        Vector2Int nextPos = paths.Peek();

        if (World.Instance.IsPositionOccupied(lastQueuePosCache))
        {
            paths.Clear();
            paths.Enqueue(currentGridPos);
            return false;
        }
        if (!World.Instance.IsPositionValid(nextPos))
        {
            ReCalculatePath();
            return false;
        }

        Vector2 pos = rb.position;

        // Always face target while moving
        

        // reached tile
        if (pos == nextPos)
        {
            paths.Dequeue();
            currentGridPos = nextPos;
            nextPos = paths.Count > 0 ? paths.Peek() : currentGridPos;
            Vector2Int delta = nextPos - currentGridPos;
            
            
            if (delta != Vector2Int.zero)
            {
                int x = delta.x;
                int y = delta.y;
                Direction dir;
                if (x > 0) dir = Direction.East;
                else if (x < 0) dir = Direction.West;
                else if (y > 0) dir = Direction.North;
                else dir = Direction.South;
                ChangeDirection(dir);
            }
            World.Instance.ChangeObjectLocation(this, oldGridPos, currentGridPos);
            oldGridPos = currentGridPos;
            return paths.Count == 0;
        }

        Vector2 tempPos =
            Vector2.MoveTowards(pos, nextPos, Time.deltaTime);

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

        int x = Random.Range(8, 22);
        int y = Random.Range(10, 20);

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
        if (!World.Instance.IsPositionValid(targetPos))
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


    //Dictionary<DiagonalDirection, DirectionData> directionToVector = new()
    //{
    //    { DiagonalDirection.North, new DirectionData(Vector2Int.up, Direction.North) },
    //    { DiagonalDirection.NorthEast, new DirectionData(new Vector2Int(1, 1), Direction.East) },
    //    { DiagonalDirection.East, new DirectionData(Vector2Int.right, Direction.East) },
    //    { DiagonalDirection.SouthEast, new DirectionData(new Vector2Int(1, -1), Direction.East) },
    //    { DiagonalDirection.South, new DirectionData(Vector2Int.down, Direction.South) },
    //    { DiagonalDirection.SouthWest, new DirectionData(new Vector2Int(-1, -1), Direction.West) },
    //    { DiagonalDirection.West, new DirectionData(Vector2Int.left, Direction.West) },
    //    { DiagonalDirection.NorthWest, new DirectionData(new Vector2Int(-1, 1), Direction.West) }
    //};
    //public void Walk(DiagonalDirection dir)
    //{
    //    DirectionData data = directionToVector[dir];
    //    ChangeDirection(data.direction);
    //    Vector2Int destination = data.vector + currentGridPosition;
    //    AddPathToQueue(destination);
    //}
}
