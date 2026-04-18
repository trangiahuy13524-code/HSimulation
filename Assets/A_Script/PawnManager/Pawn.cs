using System.Collections.Generic;
using UnityEngine;

public class Pawn : ObjectBase
{
    [SerializeField] BodyData bodyData;
    [SerializeField] HeadData headData;
    [SerializeField] HairData hairData;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Vector2Int lastQueuePosCache;
    Queue<Vector2Int> paths;

    public BodyData BodyData => bodyData;
    public HeadData HeadData => headData;
    public HairData HairData => hairData;

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

    Dictionary<DiagonalDirection, DirectionData> directionToVector = new()
    {
        { DiagonalDirection.North, new DirectionData(Vector2Int.up, Direction.North) },
        { DiagonalDirection.NorthEast, new DirectionData(new Vector2Int(1, 1), Direction.East) },
        { DiagonalDirection.East, new DirectionData(Vector2Int.right, Direction.East) },
        { DiagonalDirection.SouthEast, new DirectionData(new Vector2Int(1, -1), Direction.East) },
        { DiagonalDirection.South, new DirectionData(Vector2Int.down, Direction.South) },
        { DiagonalDirection.SouthWest, new DirectionData(new Vector2Int(-1, -1), Direction.West) },
        { DiagonalDirection.West, new DirectionData(Vector2Int.left, Direction.West) },
        { DiagonalDirection.NorthWest, new DirectionData(new Vector2Int(-1, 1), Direction.West) }
    };

    Dictionary<Vector2Int, Direction> vectorToDirection = new()
    {
        { Vector2Int.up, Direction.North },
        { new Vector2Int(1, 1), Direction.East },
        { Vector2Int.right, Direction.East },
        { new Vector2Int(1, -1), Direction.East },
        { Vector2Int.down, Direction.South },
        { new Vector2Int(-1, -1), Direction.West },
        { Vector2Int.left, Direction.West },
        { new Vector2Int(-1, 1), Direction.West }
    };

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

    public void ChangeDirection(Direction dir)
    {
        if (bodyData) bodyData.SetDirection(dir);
        if (headData) headData.SetDirection(dir);
        if (hairData) hairData.SetDirection(dir);
    }

    public void Walk(DiagonalDirection dir)
    {
        DirectionData data = directionToVector[dir];
        ChangeDirection(data.direction);
        Vector2Int destination = data.vector + currentGridPosition;
        AddPathToQueue(destination);
    }


    private void Update()
    {
        int pathCount = paths.Count;
        if (pathCount == 0)
        {
            return;
        }
        
        Vector2Int destinationGridPos = paths.Peek();

        Vector2 pos = rb.position;

        if (pos == destinationGridPos)
        {
            currentGridPosition = destinationGridPos;
            paths.Dequeue();
            if (pathCount > 1)
            {
                Vector2Int nextDestination = paths.Peek();
                Vector2Int delta = nextDestination - currentGridPosition;
                Direction dir = vectorToDirection[delta];
                ChangeDirection(dir);
            }
            return;
        }

        Vector2 tempPos = Vector2.MoveTowards(pos, destinationGridPos, Time.deltaTime);
        rb.MovePosition(tempPos);
    }



    protected override void Start()
    {
        base.Start();
        paths = new Queue<Vector2Int>();
    }
}
