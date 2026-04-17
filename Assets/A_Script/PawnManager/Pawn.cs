using System.Collections.Generic;
using System;
using UnityEngine;

public class Pawn : ObjectBase
{
    [SerializeField] BodyData bodyData;
    [SerializeField] HeadData headData;
    [SerializeField] HairData hairData;
    [SerializeField] Rigidbody2D rb;
    public Queue<Vector2Int> paths = new();

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
        Vector2Int delta = data.vector;
        paths.Enqueue(currentGridPosition + delta);
    }


    private void Update()
    {
        if (paths.Count == 0)
        {
            return;
        }
        
        Vector2Int destinationGridPos = paths.Peek();

        Vector2 pos = rb.position;

        if (pos == destinationGridPos)
        {
            currentGridPosition = destinationGridPos;
            paths.Dequeue();
            return;
        }

        Vector2 tempPos = Vector2.MoveTowards(pos, destinationGridPos, Time.deltaTime);
        rb.MovePosition(tempPos);
    }

    

    //protected override void Start()
    //{
    //    base.Start();
    //    Vector2Int pos = currentGridPos;
    //    pos += Vector2Int.left;
    //    paths.Enqueue(pos);
    //    pos += Vector2Int.left;
    //    paths.Enqueue(pos);
    //    pos += Vector2Int.left + Vector2Int.up;
    //    paths.Enqueue(pos);
    //    pos += Vector2Int.up;
    //    paths.Enqueue(pos);
    //    pos += Vector2Int.up + Vector2Int.right;
    //    paths.Enqueue(pos);
    //    pos += Vector2Int.right;
    //    paths.Enqueue(pos);
    //    pos += Vector2Int.right + Vector2Int.down;
    //    paths.Enqueue(pos);
    //    pos += Vector2Int.down;
    //    paths.Enqueue(pos);
    //    pos += Vector2Int.down;
    //    paths.Enqueue(pos);
    //}
}
