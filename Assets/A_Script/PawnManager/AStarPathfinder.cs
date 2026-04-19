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

        Dictionary<Vector2Int, Node> nodes = new();
        List<Node> openList = new();
        HashSet<Vector2Int> closed = new();

        Node startNode = new(start);
        Node targetNode = new(target);

        openList.Add(startNode);
        nodes[start] = startNode;

        while (openList.Count > 0)
        {
            Node current = GetLowestFCost(openList);

            if (current.position == target)
                return SmoothPath(RetracePath(current));

            openList.Remove(current);
            closed.Add(current.position);

            foreach (var dir in directions)
            {
                Vector2Int neighborPos = current.position + dir;

                // target blocked
                if (!world.IsPositionValid(neighborPos))
                    continue;

                // Prevent diagonal corner cutting
                if (dir.x != 0 && dir.y != 0)
                {
                    Vector2Int sideA =
                        current.position + new Vector2Int(dir.x, 0);

                    Vector2Int sideB =
                        current.position + new Vector2Int(0, dir.y);

                    if (!world.IsPositionValid(sideA) ||
                        !world.IsPositionValid(sideB))
                        continue;
                }

                if (closed.Contains(neighborPos))
                    continue;

                int moveCost = current.gCost + GetDistance(current.position, neighborPos);

                if (!nodes.TryGetValue(neighborPos, out Node neighbor))
                {
                    neighbor = new Node(neighborPos);
                    nodes[neighborPos] = neighbor;
                }

                if (moveCost < neighbor.gCost || !openList.Contains(neighbor))
                {
                    neighbor.gCost = moveCost;
                    neighbor.hCost = GetDistance(neighborPos, target);
                    neighbor.parent = current;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        return null; // no path
    }

    static Node GetLowestFCost(List<Node> list)
    {
        Node best = list[0];

        for (int i = 1; i < list.Count; i++)
        {
            if (list[i].fCost < best.fCost ||
               (list[i].fCost == best.fCost && list[i].hCost < best.hCost))
            {
                best = list[i];
            }
        }

        return best;
    }

    static List<Vector2Int> RetracePath(Node endNode)
    {
        List<Vector2Int> path = new();

        Node current = endNode;

        while (current.parent != null)
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

        return 14 * Mathf.Min(dx, dy) + 10 * Mathf.Abs(dx - dy);
    }

    //static bool LineWalkable(Vector2Int a, Vector2Int b)
    //{
    //    World world = World.Instance;

    //    int dx = Mathf.Abs(b.x - a.x);
    //    int dy = Mathf.Abs(b.y - a.y);

    //    int sx = a.x < b.x ? 1 : -1;
    //    int sy = a.y < b.y ? 1 : -1;

    //    int err = dx - dy;

    //    Vector2Int current = a;
    //    Vector2Int previous = a;

    //    while (true)
    //    {
    //        if (!world.IsPositionValid(current))
    //            return false;

    //        // Prevent diagonal corner cutting during smoothing
    //        Vector2Int delta = current - previous;

    //        if (delta.x != 0 && delta.y != 0)
    //        {
    //            Vector2Int sideA =
    //                previous + new Vector2Int(delta.x, 0);

    //            Vector2Int sideB =
    //                previous + new Vector2Int(0, delta.y);

    //            if (!world.IsPositionValid(sideA) ||
    //                !world.IsPositionValid(sideB))
    //                return false;
    //        }

    //        if (current == b)
    //            break;

    //        previous = current;

    //        int e2 = err * 2;

    //        if (e2 > -dy)
    //        {
    //            err -= dy;
    //            current.x += sx;
    //        }

    //        if (e2 < dx)
    //        {
    //            err += dx;
    //            current.y += sy;
    //        }
    //    }

    //    return true;
    //}

    static List<Vector2Int> SmoothPath(List<Vector2Int> path)
    {
        if (path == null || path.Count < 3)
            return path;

        List<Vector2Int> result = new();
        result.Add(path[0]);

        Vector2Int lastDir = path[1] - path[0];

        for (int i = 2; i < path.Count; i++)
        {
            Vector2Int newDir = path[i] - path[i - 1];

            // direction changed → keep corner
            if (newDir != lastDir)
            {
                result.Add(path[i - 1]);
                lastDir = newDir;
            }
        }

        result.Add(path[^1]);

        return result;
    }
}