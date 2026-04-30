using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public partial class Pawn : WorldObject
{
    public GameObject hightlight;

    [SerializeField] BodyData bodyData;
    [SerializeField] HeadData headData;
    [SerializeField] HairData hairData;
    [SerializeField] FacialAnimator facial;
    [SerializeField] Direction oldDirection = Direction.South;
    public BodyData BodyData => bodyData;
    public HeadData HeadData => headData;
    public HairData HairData => hairData;
    public FacialAnimator Facial => facial;


    [SerializeField] Rigidbody2D rb;
    [SerializeField] Vector2Int lastQueuePosCache;
    [SerializeField] float idleTime = 2f;
    [SerializeField] float currentIdleTime = 0f;
    Queue<Vector2Int> paths;
    [SerializeField] Vector2Int oldDestination;
    [SerializeField] Vector2Int oldGridPos;

    public bool onDuty = false;
    public void ChangeDirection(Direction dir)
    {
        if (dir == oldDirection) return;
        oldDirection = dir;
        if (bodyData) bodyData.SetDirection(dir);
        if (headData) headData.SetDirection(dir);
        if (hairData) hairData.SetDirection(dir);
        if (facial) facial.SetDirection(dir);
    }



    private void OnDestroy()
    {
        if (world != null) world.ModifyPawnCountGrid(currentGridPos, false);
    }
    protected override void Start()
    {
        transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);
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
        if (paths.Count > 0)
        {
            Vector2Int nextPos = paths.Peek();

            if (nextPos != currentGridPos && !world.IsPositionPathValid(nextPos))
            {
                ReCalculatePath();
                return false;
            }
            Vector2Int delta = nextPos - currentGridPos;
            int x = delta.x;
            int y = delta.y;
            if (x != 0 && y != 0)
            {
                if (!world.IsPositionPathValid(nextPos - new Vector2Int(x, 0)) || !world.IsPositionPathValid(nextPos - new Vector2Int(0, y)))
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
                    return true;
                }
            }
            float moveSpeed = Time.deltaTime * genome.speed;
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

        

        return true;
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
        if (!world.IsPositionPathValid(targetPos))
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
