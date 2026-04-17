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

    public void ChangeDirection(Direction dir)
    {
        if (bodyData) bodyData.SetDirection(dir);
        if (headData) headData.SetDirection(dir);
        if (hairData) hairData.SetDirection(dir);
    }

    public void Walk(DiagonalDirection dir)
    {
        Vector2Int delta = dir switch
        {
            DiagonalDirection.up => Vector2Int.up,
            DiagonalDirection.upright => Vector2Int.up + Vector2Int.right,
            DiagonalDirection.right => Vector2Int.right,
            DiagonalDirection.downright => Vector2Int.down + Vector2Int.right,
            DiagonalDirection.down => Vector2Int.down,
            DiagonalDirection.downleft => Vector2Int.down + Vector2Int.left,
            DiagonalDirection.left => Vector2Int.left,
            DiagonalDirection.upleft => Vector2Int.up + Vector2Int.left,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
        paths.Enqueue(currentGridPos + delta);
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
            currentGridPos = destinationGridPos;
            paths.Dequeue();
            return;
        }

        Vector2 tempPos = Vector2.MoveTowards(pos, destinationGridPos, Time.deltaTime);
        rb.MovePosition(tempPos);

        Vector2Int delta = destinationGridPos - currentGridPos;

        if (delta.x != 0)
            ChangeDirection(delta.x > 0 ? Direction.East : Direction.West);
        else
            ChangeDirection(delta.y > 0 ? Direction.North : Direction.South);

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
