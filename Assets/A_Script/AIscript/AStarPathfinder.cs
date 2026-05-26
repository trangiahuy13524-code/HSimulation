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

    static Node[,] nodes;
    static PriorityQueue<Node> openQueue;

    static int cachedSize = -1;
    static int currentSearchId = 0;

    const int MAX_ITERATIONS = 5000;

    // =====================================================
    // MAIN ENTRY
    // =====================================================

    public static List<Vector2Int> FindPath(
        Vector2Int start,
        Vector2Int target,
        byte maxRange)
    {
        World world = World.Instance;
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
            GetDistance(start.x, start.y, target.x, target.y);

        openQueue.Enqueue(startNode, startNode.fCost);

        Node bestNode = startNode; // ⭐ fallback result

        int iterations = 0;

        while (openQueue.Count > 0)
        {
            if (++iterations > MAX_ITERATIONS)
                break;

            Node current;

            do
            {
                if (openQueue.Count == 0)
                    return BuildPath(bestNode);

                current = openQueue.Dequeue();

            } while (current.closedId == currentSearchId);

            current.closedId = currentSearchId;

            // ⭐ Track best reachable node
            if (current.hCost < bestNode.hCost)
                bestNode = current;

            // reached target
            if (current.x == target.x &&
                current.y == target.y)
            {
                return BuildPath(current);
            }

            foreach (var dir in directions)
            {
                int nx = current.x + dir.x;
                int ny = current.y + dir.y;

                if (!IsInside(nx, ny, size))
                    continue;

                // ⭐ Manhattan search limit
                int manhattan =
                    Mathf.Abs(nx - start.x) +
                    Mathf.Abs(ny - start.y);

                if (manhattan > maxRange)
                    continue;

                Vector2Int neighborPos = new(nx, ny);

                if (!world.IsPositionPathValid(neighborPos))
                    continue;

                // prevent diagonal corner cutting
                if (dir.x != 0 && dir.y != 0)
                {
                    if (world.IsNotPassable(
                        new Vector2Int(current.x + dir.x, current.y)) ||
                        world.IsNotPassable(
                        new Vector2Int(current.x, current.y + dir.y)))
                        continue;
                }

                Node neighbor = GetNode(nx, ny);
                InitNode(neighbor);

                if (neighbor.closedId == currentSearchId)
                    continue;

                int newCost =
                    current.gCost +
                    GetDistance(current.x, current.y, nx, ny);

                if (newCost < neighbor.gCost)
                {
                    neighbor.gCost = newCost;
                    neighbor.hCost =
                        GetDistance(nx, ny, target.x, target.y);

                    neighbor.parent = current;

                    openQueue.Enqueue(neighbor, neighbor.fCost);
                }
            }
        }

        // ⭐ RETURN BEST POSSIBLE PATH
        return BuildPath(bestNode);
    }

    public static List<Vector2Int> FindPath(
    Vector2Int start,
    Vector2Int target,
    byte maxRange,
    Vector2Int currentPawnPos)
    {
        World world = World.Instance;
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
            GetDistance(start.x, start.y, target.x, target.y);

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
                    return BuildPath(bestNode);

                current = openQueue.Dequeue();

            } while (current.closedId == currentSearchId);

            current.closedId = currentSearchId;

            if (current.hCost < bestNode.hCost)
                bestNode = current;

            if (current.x == target.x &&
                current.y == target.y)
            {
                return BuildPath(current);
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

                // ⭐ IMPORTANT FIX
                bool isOwnPawnTile = neighborPos == currentPawnPos;

                if (!isOwnPawnTile &&
                    !world.IsPositionPathValid(neighborPos))
                    continue;

                // prevent diagonal corner cutting
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
                    GetDistance(current.x, current.y, nx, ny);

                if (newCost < neighbor.gCost)
                {
                    neighbor.gCost = newCost;
                    neighbor.hCost =
                        GetDistance(nx, ny, target.x, target.y);

                    neighbor.parent = current;

                    openQueue.Enqueue(neighbor, neighbor.fCost);
                }
            }
        }

        return BuildPath(bestNode);
    }

    public static WorkPosition FindReachableWorkPosition(
    Vector2Int start,
    IReadOnlyList<WorkPosition> workPositions,
    byte maxRange)
    {
        if (workPositions == null || workPositions.Count == 0)
            return null;

        World world = World.Instance;
        int size = world.WorldSize;

        if (!IsInside(start, size))
            return null;

        EnsureBuffers(size);

        currentSearchId++;
        openQueue.Clear();

        Node startNode = GetNode(start.x, start.y);
        InitNode(startNode);

        startNode.gCost = 0;

        // heuristic = closest work position
        int bestH = int.MaxValue;

        for (int i = 0; i < workPositions.Count; i++)
        {
            WorkPosition wp = workPositions[i];

            if (wp == null || wp.occupied)
                continue;

            int h = GetDistance(
                start.x,
                start.y,
                wp.workPos.x,
                wp.workPos.y);

            if (h < bestH)
                bestH = h;
        }

        startNode.hCost = bestH;

        openQueue.Enqueue(startNode, startNode.fCost);

        int iterations = 0;
        const int MAX_ITERATIONS = 5000;

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

            // SUCCESS CHECK
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

                Vector2Int neighborPos = new(nx, ny);

                if (!world.IsPositionPathValid(neighborPos))
                    continue;

                // diagonal corner cutting
                if (dir.x != 0 && dir.y != 0)
                {
                    if (world.IsNotPassable(
                        new Vector2Int(current.x + dir.x, current.y)) ||
                        world.IsNotPassable(
                        new Vector2Int(current.x, current.y + dir.y)))
                        continue;
                }

                Node neighbor = GetNode(nx, ny);
                InitNode(neighbor);

                if (neighbor.closedId == currentSearchId)
                    continue;

                int newCost =
                    current.gCost +
                    GetDistance(current.x, current.y, nx, ny);

                if (newCost < neighbor.gCost)
                {
                    neighbor.gCost = newCost;

                    // closest target heuristic
                    int closestH = int.MaxValue;

                    for (int i = 0; i < workPositions.Count; i++)
                    {
                        WorkPosition wp = workPositions[i];

                        if (wp == null || wp.occupied)
                            continue;

                        int h = GetDistance(
                            nx,
                            ny,
                            wp.workPos.x,
                            wp.workPos.y);

                        if (h < closestH)
                            closestH = h;
                    }

                    neighbor.hCost = closestH;

                    neighbor.parent = current;

                    openQueue.Enqueue(neighbor, neighbor.fCost);
                }
            }
        }

        return null;
    }

    public static List<Vector2Int> FindPathWithoutLast(
    Vector2Int start,
    Vector2Int target,
    byte maxRange)
    {
        List<Vector2Int> path =
            FindPath(start, target, maxRange);

        if (path == null || path.Count <= 1)
            return path;

        // remove final destination tile
        path.RemoveAt(path.Count - 1);

        return path;
    }

    // =====================================================
    // BUFFERS
    // =====================================================

    static void EnsureBuffers(int size)
    {
        if (cachedSize == size)
            return;

        nodes = new Node[size, size];
        openQueue = new PriorityQueue<Node>();

        cachedSize = size;
    }

    static Node GetNode(int x, int y)
    {
        Node n = nodes[x, y];

        if (n == null)
        {
            n = new Node { x = x, y = y };
            nodes[x, y] = n;
        }

        return n;
    }

    static void InitNode(Node n)
    {
        if (n.searchId == currentSearchId)
            return;

        n.searchId = currentSearchId;
        n.closedId = -1;
        n.gCost = int.MaxValue;
        n.parent = null;
    }

    // =====================================================
    // PATH BUILD
    // =====================================================

    static List<Vector2Int> BuildPath(Node endNode)
    {
        if (endNode == null)
            return null;

        List<Vector2Int> path = new();

        Node current = endNode;

        while (current != null)
        {
            path.Add(new Vector2Int(current.x, current.y));
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    // =====================================================
    // HELPERS
    // =====================================================

    static bool IsInside(Vector2Int pos, int size)
        => pos.x >= 0 && pos.y >= 0 &&
           pos.x < size && pos.y < size;

    static bool IsInside(int x, int y, int size)
        => x >= 0 && y >= 0 &&
           x < size && y < size;

    static int GetDistance(int ax, int ay, int bx, int by)
    {
        int dx = Mathf.Abs(ax - bx);
        int dy = Mathf.Abs(ay - by);

        return 14 * Mathf.Min(dx, dy)
             + 10 * Mathf.Abs(dx - dy);
    }
}