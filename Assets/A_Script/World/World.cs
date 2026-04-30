using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class World : MonoBehaviour
{
    public static World Instance;

    [SerializeField] byte gameFPS = 48;
    [SerializeField] Tilemap terrainTilemap;
    [SerializeField] Tilemap wallTileMap;
    [SerializeField] Transform wallDummies;
    [SerializeField] GameObject wallDummyPrefab;
    [SerializeField] GameObject pawnPrefab;
    [SerializeField] Transform cam;
    [SerializeField] short worldSize = 50;
    

    WorldObject[,] objects;

    byte[,] pawnCountOnGrid;

    public short WorldSize => worldSize;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        Application.targetFrameRate = gameFPS;

        objects = new WorldObject[worldSize, worldSize];
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


    

    public bool IsPositionPathValid(Vector2Int position)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return false;
        if (pawnCountOnGrid[x, y] > 1) return false;
        WorldObject @object = objects[x, y];
        if (@object != null)
        {
            if (!@object.IsPassable) return false;
        }
        return true;
    }

    public bool IsNotPassable(Vector2Int position)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return true;
        WorldObject @object = objects[x, y];
        if (@object != null)
        {
            if (!@object.IsPassable) return true;
        }
        return false;
    }

    public bool RegisterObject(WorldObject obj, Vector2Int position)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return false;
        if (objects[x, y] != null) return false;
        objects[x, y] = obj;
        return true;
    }
    public void UnregisterObject(Vector2Int position)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return;
        objects[x, y] = null;
    }
    public void UnregisterObject(Vector2Int position, WorldObject obj)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return;
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
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return null;
        return objects[x, y];
    }

    public void SetWallTile(Vector2Int position, Tile tile)
    {
        int x = position.x;
        int y = position.y;
        if (wallTileMap == null) return;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return;
        wallTileMap.SetTile(new Vector3Int(x, y, 0), tile);
        RefreshNeighborWall(x, y);
    }

    public void ModifyPawnCountGrid(Vector2Int position, bool add)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return;
        if (add)
            pawnCountOnGrid[x, y]++;
        else
            pawnCountOnGrid[x, y]--;
    }

    public byte GetPawnCount(Vector2Int position)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return 0;
        return pawnCountOnGrid[x, y];
    }

    public void GenerateWall(Vector2Int position, AutoTillingTile wallTile)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return;
        if (pawnCountOnGrid[x, y] > 0) return;
        WorldObject existingObject = objects[x, y];
        if (objects[x, y] != null) return; 
        wallTileMap.SetTile(new Vector3Int(x, y, 0), wallTile);
        RefreshNeighborWall(x, y);
        Transform dummyTf = Instantiate(wallDummyPrefab).transform;
        dummyTf.parent = wallDummies;
        Wall dummy = dummyTf.GetComponent<Wall>();
        dummy.CurrentGridPosition = position;
    }

    public void RemoveObject(Vector2Int position)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return;
        WorldObject ob = objects[x, y];
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

    public Pawn GeneratePawn(Vector2Int position, GeneticData geneticData)
    {
        if (pawnPrefab == null || geneticData == null) return null;
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return null;
        if (pawnCountOnGrid[x, y] > 1) return null;
        WorldObject existingObject = objects[x, y];
        if (existingObject != null)
        {
            if (!existingObject.IsPassable) return null;
        }
        Pawn pawn = Instantiate(pawnPrefab).GetComponent<Pawn>();
        if (pawn == null) return null;
        pawn.InitializePawn(geneticData);
        pawn.CurrentGridPosition = position;
        pawn.transform.position = new Vector3Int(x, y, 0);
        return pawn;
    }
}