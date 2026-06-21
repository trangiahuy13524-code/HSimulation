using System.Collections.Generic;
using UnityEngine;

public static class AStarPathfinder
{
    // =====================================================
    // NODE
    // =====================================================

    class Node
    {
        public int x;
        public int y;

        public int gCost;
        public int hCost;

        public Node parent;

        public int searchId;
        public int closedId;

        public int fCost => gCost + hCost;
    }

    // =====================================================

    static readonly Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,

        new Vector2Int(1,1),
        new Vector2Int(1,-1),
        new Vector2Int(-1,1),
        new Vector2Int(-1,-1)
    };

    // =====================================================

    [System.ThreadStatic]
    static Node[,] nodes;

    [System.ThreadStatic]
    static PriorityQueue<Node> openQueue;

    [System.ThreadStatic]
    static int cachedSize;

    [System.ThreadStatic]
    static int currentSearchId;

    const int MAX_ITERATIONS = 500;

    // =====================================================
    // NORMAL PATH
    // =====================================================

    public static List<Vector2Int> FindPath(
        Vector2Int start,
        Vector2Int target,
        byte maxRange,
        WorldThreadSafe world)
    {
        return InternalFindPath(
            start,
            target,
            maxRange,
            world,
            false,
            default,
            false);
    }

    // =====================================================
    // PATH ALLOWING CURRENT PAWN TILE
    // =====================================================

    public static List<Vector2Int> FindPath(
        Vector2Int start,
        Vector2Int target,
        byte maxRange,
        Vector2Int currentPawnPos,
        WorldThreadSafe world)
    {
        return InternalFindPath(
            start,
            target,
            maxRange,
            world,
            true,
            currentPawnPos,
            false);
    }

    // =====================================================
    // PATH WITHOUT LAST TILE
    // =====================================================

    public static List<Vector2Int> FindPathWithoutLast(
        Vector2Int start,
        Vector2Int target,
        byte maxRange,
        WorldThreadSafe world)
    {
        List<Vector2Int> path =
            InternalFindPath(
                start,
                target,
                maxRange,
                world,
                false,
                default,
                true);

        return path;
    }

    // =====================================================
    // INTERNAL
    // =====================================================

    static List<Vector2Int> InternalFindPath(
        Vector2Int start,
        Vector2Int target,
        byte maxRange,
        WorldThreadSafe world,
        bool allowPawnTile,
        Vector2Int currentPawnPos,
        bool removeLast)
    {
        int size = world.WorldSize;

        if (!IsInside(start, size))
            return null;

        EnsureBuffers(size);

        currentSearchId++;
        openQueue.Clear();

        Node startNode = GetNode(start.x, start.y);

        InitNode(startNode);

        startNode.gCost = 0;
        startNode.hCost =
            GetDistance(
                start.x,
                start.y,
                target.x,
                target.y);

        openQueue.Enqueue(startNode, startNode.fCost);

        Node bestNode = startNode;

        int iterations = 0;

        while (openQueue.Count > 0)
        {
            if (++iterations > MAX_ITERATIONS)
                break;

            Node current;

            do
            {
                if (openQueue.Count == 0)
                    return BuildFinalPath(bestNode, removeLast);

                current = openQueue.Dequeue();

            } while (current.closedId == currentSearchId);

            current.closedId = currentSearchId;

            if (current.hCost < bestNode.hCost)
                bestNode = current;

            // reached target
            if (current.x == target.x &&
                current.y == target.y)
            {
                return BuildFinalPath(current, removeLast);
            }

            foreach (var dir in directions)
            {
                int nx = current.x + dir.x;
                int ny = current.y + dir.y;

                if (!IsInside(nx, ny, size))
                    continue;

                int manhattan =
                    Mathf.Abs(nx - start.x) +
                    Mathf.Abs(ny - start.y);

                if (manhattan > maxRange)
                    continue;

                Vector2Int neighborPos = new(nx, ny);

                bool isOwnPawnTile =
                    allowPawnTile &&
                    neighborPos == currentPawnPos;

                if (!isOwnPawnTile &&
                    !world.IsPositionPathValid(neighborPos))
                    continue;

                // prevent diagonal cutting
                if (dir.x != 0 && dir.y != 0)
                {
                    Vector2Int sideA =
                        new(current.x + dir.x, current.y);

                    Vector2Int sideB =
                        new(current.x, current.y + dir.y);

                    if ((!isOwnPawnTile &&
                         world.IsNotPassable(sideA)) ||
                        (!isOwnPawnTile &&
                         world.IsNotPassable(sideB)))
                        continue;
                }

                Node neighbor = GetNode(nx, ny);

                InitNode(neighbor);

                if (neighbor.closedId == currentSearchId)
                    continue;

                int newCost =
                    current.gCost +
                    GetDistance(
                        current.x,
                        current.y,
                        nx,
                        ny);

                if (newCost < neighbor.gCost)
                {
                    neighbor.gCost = newCost;

                    neighbor.hCost =
                        GetDistance(
                            nx,
                            ny,
                            target.x,
                            target.y);

                    neighbor.parent = current;

                    openQueue.Enqueue(
                        neighbor,
                        neighbor.fCost);
                }
            }
        }

        return BuildFinalPath(bestNode, removeLast);
    }

    // =====================================================
    // FIND REACHABLE WORK POSITION
    // =====================================================

    public static WorkPosition FindReachableWorkPosition(
        Vector2Int start,
        IReadOnlyList<WorkPosition> workPositions,
        byte maxRange,
        WorldThreadSafe world)
    {
        if (workPositions == null ||
            workPositions.Count == 0)
            return null;

        int size = world.WorldSize;

        if (!IsInside(start, size))
            return null;

        EnsureBuffers(size);

        currentSearchId++;
        openQueue.Clear();

        Node startNode = GetNode(start.x, start.y);

        InitNode(startNode);

        startNode.gCost = 0;
        startNode.hCost = 0;

        openQueue.Enqueue(startNode, 0);

        int iterations = 0;

        while (openQueue.Count > 0)
        {
            if (++iterations > MAX_ITERATIONS)
                break;

            Node current;

            do
            {
                if (openQueue.Count == 0)
                    return null;

                current = openQueue.Dequeue();

            } while (current.closedId == currentSearchId);

            current.closedId = currentSearchId;

            // reached work position
            for (int i = 0; i < workPositions.Count; i++)
            {
                WorkPosition wp = workPositions[i];

                if (wp == null || wp.occupied)
                    continue;

                if (current.x == wp.workPos.x &&
                    current.y == wp.workPos.y)
                {
                    return wp;
                }
            }

            foreach (var dir in directions)
            {
                int nx = current.x + dir.x;
                int ny = current.y + dir.y;

                if (!IsInside(nx, ny, size))
                    continue;

                int manhattan =
                    Mathf.Abs(nx - start.x) +
                    Mathf.Abs(ny - start.y);

                if (manhattan > maxRange)
                    continue;

                Vector2Int next = new(nx, ny);

                if (!world.IsPositionPathValid(next) && next != start)
                    continue;

                // diagonal cutting
                if (dir.x != 0 && dir.y != 0)
                {
                    Vector2Int sideA =
                        new(current.x + dir.x, current.y);

                    Vector2Int sideB =
                        new(current.x, current.y + dir.y);

                    if (world.IsNotPassable(sideA) ||
                        world.IsNotPassable(sideB))
                        continue;
                }

                Node neighbor = GetNode(nx, ny);

                InitNode(neighbor);

                if (neighbor.closedId == currentSearchId)
                    continue;

                int newCost =
                    current.gCost +
                    GetDistance(
                        current.x,
                        current.y,
                        nx,
                        ny);

                if (newCost < neighbor.gCost)
                {
                    neighbor.gCost = newCost;
                    neighbor.hCost = 0;
                    neighbor.parent = current;

                    openQueue.Enqueue(
                        neighbor,
                        neighbor.fCost);
                }
            }
        }

        return null;
    }

    // =====================================================
    // BUFFERS
    // =====================================================

    static void EnsureBuffers(int size)
    {
        if (nodes != null &&
            cachedSize == size)
            return;

        nodes = new Node[size, size];

        openQueue = new PriorityQueue<Node>();

        cachedSize = size;
    }

    static Node GetNode(int x, int y)
    {
        Node node = nodes[x, y];

        if (node == null)
        {
            node = new Node
            {
                x = x,
                y = y
            };

            nodes[x, y] = node;
        }

        return node;
    }

    static void InitNode(Node node)
    {
        if (node.searchId == currentSearchId)
            return;

        node.searchId = currentSearchId;
        node.closedId = -1;

        node.gCost = int.MaxValue;
        node.hCost = 0;

        node.parent = null;
    }

    // =====================================================
    // BUILD PATH
    // =====================================================

    static List<Vector2Int> BuildFinalPath(
        Node endNode,
        bool removeLast)
    {
        List<Vector2Int> path =
            BuildPath(endNode);

        if (removeLast &&
            path != null &&
            path.Count > 1)
        {
            path.RemoveAt(path.Count - 1);
        }

        return path;
    }

    static List<Vector2Int> BuildPath(Node endNode)
    {
        if (endNode == null)
            return null;

        List<Vector2Int> path = new();

        Node current = endNode;

        while (current != null)
        {
            path.Add(
                new Vector2Int(
                    current.x,
                    current.y));

            current = current.parent;
        }

        path.Reverse();

        return path;
    }

    // =====================================================
    // HELPERS
    // =====================================================

    static bool IsInside(Vector2Int pos, int size)
    {
        return pos.x >= 0 &&
               pos.y >= 0 &&
               pos.x < size &&
               pos.y < size;
    }

    static bool IsInside(int x, int y, int size)
    {
        return x >= 0 &&
               y >= 0 &&
               x < size &&
               y < size;
    }

    static int GetDistance(
        int ax,
        int ay,
        int bx,
        int by)
    {
        int dx = Mathf.Abs(ax - bx);
        int dy = Mathf.Abs(ay - by);

        return 14 * Mathf.Min(dx, dy)
             + 10 * Mathf.Abs(dx - dy);
    }
}