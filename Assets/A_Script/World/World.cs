using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class World : MonoBehaviour
{
    public static World Instance;
    [SerializeField] MapRenderer mapRenderer;
    [SerializeField] PlaceableTile wallDummyTile;

    [SerializeField] byte gameFPS = 48;
    [SerializeField] Tilemap terrainTilemap;
    [SerializeField] Tilemap wallTileMap;
    [SerializeField] Transform wallDummies;
    [SerializeField] GameObject wallDummyPrefab;
    [SerializeField] GameObject pawnPrefab;
    [SerializeField] Transform cam;
    [SerializeField] short worldSize = 50;

    [SerializeField] byte maxPawnCount = 2;
    WorldObject[,] objects;

    byte[,] pawnCountOnGrid;

    public short WorldSize => worldSize;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        Application.targetFrameRate = gameFPS;

        objects = new WorldObject[worldSize, worldSize];
        if (maxPawnCount == 0) maxPawnCount = 1;
        pawnCountOnGrid = new byte[worldSize, worldSize];
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                pawnCountOnGrid[x, y] = 0;
            }
        }
        if (cam) cam.position = new Vector3((worldSize - 1) / 2f, (worldSize - 2) / 2f, cam.position.z);
    }



    static readonly Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };
    public bool IsInside(Vector2Int p)
    {
        return p.x >= 0 && p.y >= 0 &&
               p.x < worldSize && p.y < worldSize;
    }
    public bool GridMaxPawn(Vector2Int position)
    {
        if (!IsInside(position)) return false;
        if (pawnCountOnGrid[position.x, position.y] >= maxPawnCount) return true;
        return false;
    }
    public bool IsPositionPathValid(Vector2Int position)
    {
        if (!IsInside(position)) return false;
        int x = position.x;
        int y = position.y;
        if (pawnCountOnGrid[x, y] >= maxPawnCount) return false;
        WorldObject @object = objects[x, y];
        if (@object != null)
        {
            if (!@object.IsPassable) return false;
        }
        return true;
    }

    public bool IsNotPassable(Vector2Int position)
    {
        if (!IsInside(position)) return true;
        WorldObject @object = objects[position.x, position.y];
        if (@object != null)
        {
            if (!@object.IsPassable) return true;
        }
        return false;
    }

    public bool RegisterObject(WorldObject obj, Vector2Int position)
    {
        if (!IsInside(position)) return false;
        int x = position.x;
        int y = position.y;
        if (objects[x, y] != null) return false;
        objects[x, y] = obj;
        return true;
    }
    public void UnregisterObject(Vector2Int position)
    {
        if (!IsInside(position)) return;
        objects[position.x, position.y] = null;
    }
    public void UnregisterObject(Vector2Int position, WorldObject obj)
    {
        if (!IsInside(position)) return;
        int x = position.x;
        int y = position.y;
        if (objects[x, y] == obj)
        {
            objects[x, y] = null;
        }
    }

    public void ChangeObjectLocation(WorldObject obj, Vector2Int oldPosition, Vector2Int newPosition)
    {
        if (obj == null) return;
        if (oldPosition == newPosition) return;
        int oldX = oldPosition.x;
        int oldY = oldPosition.y;
        int newX = newPosition.x;
        int newY = newPosition.y;
        if (oldX < 0 || oldX >= worldSize || oldY < 0 || oldY >= worldSize) return;
        if (newX < 0 || newX >= worldSize || newY < 0 || newY >= worldSize) return;
        if (objects[newX, newY] != null) return;
        objects[newX, newY] = obj;
        objects[oldX, oldY] = null;
    }

    public WorldObject GetObjectAtPosition(Vector2Int position)
    {
        if (!IsInside(position)) return null;
        return objects[position.x, position.y];
    }

    public void SetWallTile(Vector2Int position, Tile tile)
    {
        if (wallTileMap == null) return;
        if (!IsInside(position)) return;
        int x = position.x;
        int y = position.y;
        wallTileMap.SetTile(new Vector3Int(x, y, 0), tile);
        RefreshNeighborWall(x, y);
    }

    public void ModifyPawnCountGrid(Vector2Int position, bool add)
    {
        if (!IsInside(position)) return;
        if (add)
            pawnCountOnGrid[position.x, position.y]++;
        else
            pawnCountOnGrid[position.x, position.y]--;
    }
    public byte GetPawnCount(Vector2Int position)
    {
        if (!IsInside(position)) return 0;
        return pawnCountOnGrid[position.x, position.y];
    }

    public void GenerateWall(Vector2Int position, AutoTillingTile wallTile)
    {
        if (!IsInside(position)) return;
        int x = position.x;
        int y = position.y;
        if (pawnCountOnGrid[x, y] > 0) return;
        WorldObject existingObject = objects[x, y];
        if (objects[x, y] != null) return;
        wallTileMap.SetTile(new Vector3Int(x, y, 0), wallTile);
        RefreshNeighborWall(x, y);
        Transform dummyTf = Instantiate(wallDummyPrefab).transform;
        dummyTf.parent = wallDummies;
        Wall dummy = dummyTf.GetComponent<Wall>();
        dummy.CurrentGridPosition = position;
        mapRenderer.map.SetTile(position, wallDummyTile);
    }

    public void RemoveObject(Vector2Int position)
    {
        if (!IsInside(position)) return;
        WorldObject ob = objects[position.x, position.y];
        if (ob == null) return;
        Destroy(ob.gameObject);
    }

    private void RefreshNeighborWall(int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                wallTileMap.RefreshTile(new Vector3Int(x + dx, y + dy, 0));
            }
        }
    }

    Queue<Vector2Int> fnaQueue = new();
    HashSet<Vector2Int> fnaVisited = new();
    public Vector2Int? FindNearestAvailable(Vector2Int start)
    {
        if (!IsInside(start))
            return null;

        fnaQueue.Clear();
        fnaVisited.Clear();

        fnaQueue.Enqueue(start);
        fnaVisited.Add(start);

        while (fnaQueue.Count > 0)
        {
            Vector2Int current = fnaQueue.Dequeue();

            WorldObject obj = objects[current.x, current.y];

            // FOUND EMPTY TILE
            if (obj == null)
                return current;

            // cannot walk through blocked tile
            if (!obj.IsPassable)
                continue;

            // explore neighbours
            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;

                if (!IsInside(next))
                    continue;

                if (!fnaVisited.Add(next))
                    continue;

                fnaQueue.Enqueue(next);
            }
        }

        return null; // no available position
    }
    public Pawn GeneratePawn(Vector2Int position, GeneticData geneticData)
    {
        if (pawnPrefab == null || geneticData == null) return null;
        if (!IsInside(position)) return null;
        int x = position.x;
        int y = position.y;
        if (pawnCountOnGrid[x, y] >= maxPawnCount) return null;
        WorldObject existingObject = objects[x, y];
        if (existingObject != null)
        {
            if (!existingObject.IsPassable) return null;
        }
        Pawn pawn = Instantiate(pawnPrefab).GetComponent<Pawn>();
        if (pawn == null) return null;
        pawn.CurrentGridPosition = position;
        pawn.transform.position = new Vector3Int(x, y, 0);
        pawn.InitializePawn(geneticData);
        return pawn;
    }
}