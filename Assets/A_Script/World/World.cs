using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

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
    float[,] noiseMap;

    ObjectBase[,] objects;

    //  
    public short WorldSize => worldSize;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        Application.targetFrameRate = gameFPS;

        objects = new ObjectBase[worldSize, worldSize];
        if (cam) cam.position = new Vector3((worldSize - 1) / 2f, (worldSize - 2) / 2f, cam.position.z);
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                int count = terrainTiles.Count;
                Tile tile = terrainTiles[Random.Range(0, count)];
                terrainTilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
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
        ObjectBase @object = objects[x, y];
        if (@object == null) return true;
        if (!@object.IsPassable) return false;
        return true;
    }

    public ObjectBase GetObjectAtPosition(Vector2Int position)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return null;
        return objects[x, y];
    }


    public void GenerateWall(AutoTillingTile wallTile, Vector2Int position)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return;
        ObjectBase existingObject = objects[x, y];
        if (objects[x, y] != null) return; 
        wallTileMap.SetTile(new Vector3Int(x, y, 0), wallTile);
        RefreshNeighborWall(x, y);
        Transform dummyTf = Instantiate(wallDummyPrefab).transform;
        dummyTf.parent = wallDummies;
        Wall dummy = dummyTf.GetComponent<Wall>();
        objects[x, y] = dummy;
    }

    public void RemoveWall(Vector2Int position)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return;
        if (objects[x, y] is not Wall wall)
            return;
        wallTileMap.SetTile(new Vector3Int(x, y, 0), null);
        RefreshNeighborWall(x, y);
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
        ObjectBase existingObject = objects[x, y];
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
    }

    public void GeneratePawn(Vector2Int position, PawnPreset preset)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= worldSize || y < 0 || y >= worldSize) return;
        ObjectBase existingObject = objects[x, y];
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
    }



    public void GenerateAttire(Pawn pawn)
    {

    }


    //public void GenerateTerrain(bool isIsland = false)
    //{

    //    worldGrid = new float[worldSize, worldSize];
    //    float scale = 0.05f;

    //    for (int y = 0; y < worldSize; y++)
    //        for (int x = 0; x < worldSize; x++)
    //        {
    //            float nx = x * scale;
    //            float ny = y * scale;

    //            float noise = FractalNoise(x * 0.01f, y * 0.01f);


    //            if (isIsland)
    //            {
    //                float island = IslandMask(x, y, worldSize);
    //                worldGrid[x, y] = noise * island;
    //            }
    //            else
    //            {
    //                worldGrid[x, y] = noise;
    //            }
    //        }
    //}

    //float FractalNoise(float x, float y)
    //{
    //    float value = 0;
    //    float amplitude = 1;
    //    float frequency = 1;
    //    float maxValue = 0;

    //    for (int i = 0; i < 4; i++)
    //    {
    //        value += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;

    //        maxValue += amplitude;
    //        amplitude *= 0.5f;
    //        frequency *= 2f;
    //    }

    //    return value / maxValue;
    //}
    //float IslandMask(int x, int y, float worldSize)
    //{
    //    float cx = worldSize / 2f;
    //    float cy = worldSize / 2f;

    //    float dx = (x - cx) / cx;
    //    float dy = (y - cy) / cy;

    //    float dist = Mathf.Sqrt(dx * dx + dy * dy);

    //    return Mathf.Clamp01(1f - dist);
    //}
    //TerrainType GetTerrain(float v)
    //{
    //    if (v < 0.35f) return TerrainType.DeepWater;
    //    if (v < 0.45f) return TerrainType.ShallowWater;
    //    if (v < 0.5f) return TerrainType.Sand;
    //    if (v < 0.75f) return TerrainType.Grass;

    //    return TerrainType.Rock;
    //}
}