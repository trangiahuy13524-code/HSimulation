using System.Collections.Generic;
using UnityEngine;

public static class AStarPathfinder
{
    class Node
    {
        public Vector2Int position;
        public int gCost;
        public int hCost;
        public Node parent;

        public int fCost => gCost + hCost;

        public Node(Vector2Int pos)
        {
            position = pos;
            gCost = int.MaxValue;
        }
    }

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

    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        World world = World.Instance;
        int size = world.WorldSize;

        // ===== Bounds check =====
        if (!IsInside(start, size) || !IsInside(target, size))
            return null;

        if (world.IsPositionOccupied(target))
            return null;

        Dictionary<Vector2Int, Node> nodes = new();
        List<Node> openList = new();
        HashSet<Vector2Int> openSet = new();
        HashSet<Vector2Int> closedSet = new();

        Node startNode = new(start)
        {
            gCost = 0,
            hCost = GetDistance(start, target)
        };

        nodes[start] = startNode;
        openList.Add(startNode);
        openSet.Add(start);

        int iterations = 0;
        const int MAX_ITERATIONS = 5000;

        while (openList.Count > 0)
        {
            if (++iterations > MAX_ITERATIONS)
            {
                Debug.LogWarning("A* aborted (iteration limit)");
                return null;
            }

            Node current = GetLowestFCost(openList);

            if (current.position == target)
                return RetracePath(current);

            openList.Remove(current);
            openSet.Remove(current.position);
            closedSet.Add(current.position);

            foreach (var dir in directions)
            {
                Vector2Int neighborPos = current.position + dir;

                // ===== HARD WORLD LIMIT =====
                if (!IsInside(neighborPos, size))
                    continue;

                if (world.IsPositionOccupied(neighborPos))
                    continue;

                if (closedSet.Contains(neighborPos))
                    continue;

                // Prevent diagonal corner cutting
                if (dir.x != 0 && dir.y != 0)
                {
                    Vector2Int sideA =
                        current.position + new Vector2Int(dir.x, 0);

                    Vector2Int sideB =
                        current.position + new Vector2Int(0, dir.y);

                    if (!IsInside(sideA, size) ||
                        !IsInside(sideB, size) ||
                        world.IsPositionOccupied(sideA) ||
                        world.IsPositionOccupied(sideB))
                        continue;
                }

                int moveCost =
                    current.gCost +
                    GetDistance(current.position, neighborPos);

                if (!nodes.TryGetValue(neighborPos, out Node neighbor))
                {
                    neighbor = new Node(neighborPos);
                    nodes[neighborPos] = neighbor;
                }

                if (moveCost < neighbor.gCost)
                {
                    neighbor.gCost = moveCost;
                    neighbor.hCost = GetDistance(neighborPos, target);
                    neighbor.parent = current;

                    if (!openSet.Contains(neighborPos))
                    {
                        openList.Add(neighbor);
                        openSet.Add(neighborPos);
                    }
                }
            }
        }

        return null;
    }

    static bool IsInside(Vector2Int pos, int size)
    {
        return pos.x >= 0 && pos.y >= 0 &&
               pos.x < size && pos.y < size;
    }

    static Node GetLowestFCost(List<Node> list)
    {
        Node best = list[0];

        for (int i = 1; i < list.Count; i++)
        {
            Node n = list[i];

            if (n.fCost < best.fCost ||
               (n.fCost == best.fCost && n.hCost < best.hCost))
                best = n;
        }

        return best;
    }

    static List<Vector2Int> RetracePath(Node endNode)
    {
        List<Vector2Int> path = new();

        Node current = endNode;

        while (current != null)
        {
            path.Add(current.position);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    static int GetDistance(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);

        return 14 * Mathf.Min(dx, dy) +
               10 * Mathf.Abs(dx - dy);
    }
}