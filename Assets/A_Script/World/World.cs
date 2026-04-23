using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class World : MonoBehaviour
{
    public static World Instance;

    [SerializeField] byte gameFPS = 48;
    [SerializeField] Tilemap terrainTilemap;
    [SerializeField] List<Tile> terrainTiles = new();
    [SerializeField] Tilemap wallTileMap;
    [SerializeField] Transform wallDummies;
    [SerializeField] GameObject wallDummyPrefab;
    [SerializeField] GameObject pawnPrefab;
    [SerializeField] Transform cam;
    [SerializeField] short worldSize = 50;
    

    BaseObject[,] objects;
    byte[,] pawnCountOnGrid;

    //  
    public short WorldSize => worldSize;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        Application.targetFrameRate = gameFPS;

        objects = new BaseObject[worldSize, worldSize];
        pawnCountOnGrid = new byte[worldSize, worldSize];
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                pawnCountOnGrid[x, y] = 0;
            }
        }
        if (cam) cam.position = new Vector3((worldSize - 1) / 2f, (worldSize - 2) / 2f, cam.position.z);

        //for (int x = 0; x < worldSize; x++)
        //{
        //    for (int y = 0; y < worldSize; y++)
        //    {
        //        int count = terrainTiles.Count;
        //        Tile tile = terrainTiles[Random.Range(0, count)];
        //        terrainTilemap.SetTile(new Vector3Int(x, y, 0), tile);
        //    }
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    

    public bool IsPositionValid(Vector2Int position)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return false;
        BaseObject @object = objects[x, y];
        if (@object == null) return true;
        if (!@object.IsPassable) return false;
        return true;
    }

    public bool IsPositionOccupied(Vector2Int position)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return false;
        BaseObject @object = objects[x, y];
        return @object != null;
    }

    public void RegisterObject(BaseObject obj, Vector2Int position)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return;
        objects[x, y] = obj;
    }
    public void UnregisterObject(Vector2Int position)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return;
        objects[x, y] = null;
    }
    public void UnregisterObject(Vector2Int position, BaseObject obj)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return;
        if (objects[x, y] == obj)
        {
            objects[x, y] = null;
        }
    }

    public void ChangeObjectLocation(BaseObject obj, Vector2Int oldPosition, Vector2Int newPosition)
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

    public BaseObject GetObjectAtPosition(Vector2Int position)
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

    public void GenerateWall(Vector2Int position, AutoTillingTile wallTile)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return;
        if (pawnCountOnGrid[x, y] > 0) return;
        BaseObject existingObject = objects[x, y];
        if (objects[x, y] != null) return; 
        wallTileMap.SetTile(new Vector3Int(x, y, 0), wallTile);
        RefreshNeighborWall(x, y);
        Transform dummyTf = Instantiate(wallDummyPrefab).transform;
        dummyTf.parent = wallDummies;
        Wall dummy = dummyTf.GetComponent<Wall>();
        dummy.CurrentGridPosition = position;
        objects[x, y] = dummy;
    }

    public void RemoveObject(Vector2Int position)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return;
        BaseObject ob = objects[x, y];
        if (ob == null) return;
        Destroy(ob.gameObject);
        objects[x, y] = null;
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

    public void GeneratePawn(Vector2Int position, DirectionSpriteData bodySprite, DirectionSpriteData headSprite = null, DirectionSpriteData hairSprite = null)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return;
        BaseObject existingObject = objects[x, y];
        if (existingObject != null) return;
        if (pawnPrefab == null) return;
        GameObject spawned = Instantiate(pawnPrefab);
        Pawn pawn = spawned.GetComponent<Pawn>();
        if (pawn == null || bodySprite == null || headSprite == null) return;
        BodyData bmanager = pawn.BodyData;
        if (bmanager) bmanager.SetDirectionSpriteData(bodySprite);
        HeadData hmanager = pawn.HeadData;
        if (hmanager) hmanager.SetDirectionSpriteData(headSprite);
        HairData hairmanager = pawn.HairData;
        if (hairmanager) hairmanager.SetDirectionSpriteData(hairSprite);
        spawned.transform.position = new Vector3Int(x, y, 0);
        objects[x, y] = pawn;
    }

    public void GeneratePawn(Vector2Int position, PawnPreset preset)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return;
        BaseObject existingObject = objects[x, y];
        if (existingObject != null) return;
        if (pawnPrefab == null) return;
        Pawn pawn = Instantiate(pawnPrefab).GetComponent<Pawn>();
        if (pawn == null || preset == null) return;
        BodyData bmanager = pawn.BodyData;
        if (bmanager) bmanager.SetDirectionSpriteData(preset.body);
        HeadData hmanager = pawn.HeadData;
        if (hmanager) hmanager.SetDirectionSpriteData(preset.head);
        HairData hairmanager = pawn.HairData;
        if (hairmanager) hairmanager.SetDirectionSpriteData(preset.hair);
        pawn.CurrentGridPosition = position;
        pawn.transform.position = new Vector3Int(x, y, 0);
        objects[x, y] = pawn;
    }

    //public void GeneratePawn(Vector2Int position, GeneticData geneticData)
    //{
    //    int x = position.x;
    //    int y = position.y;
    //    if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return;
    //    BaseObject existingObject = objects[x, y];
    //    if (existingObject != null) return;
    //    if (pawnPrefab == null) return;
    //    Pawn pawn = Instantiate(pawnPrefab).GetComponent<Pawn>();
    //    if (pawn == null || geneticData == null) return;
    //    //BodyData bmanager = pawn.BodyData;
    //    //if (bmanager) bmanager.SetDirectionSpriteData(geneticData.bodyData);
    //    //HeadData hmanager = pawn.HeadData;
    //    //if (hmanager) hmanager.SetDirectionSpriteData(geneticData.headData);
    //    //HairData hairmanager = pawn.HairData;
    //    //if (hairmanager) hairmanager.SetDirectionSpriteData(geneticData.hairData);
    //    pawn.CurrentGridPosition = position;
    //    pawn.transform.position = new Vector3Int(x, y, 0);
    //    objects[x, y] = pawn;
    //}

    public static void GenerateAttire(Pawn pawn)
    {

    }


    
    
}