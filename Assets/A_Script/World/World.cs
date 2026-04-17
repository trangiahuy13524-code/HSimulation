using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class World : MonoBehaviour
{
    [SerializeField] byte gameFPS = 48;
    [SerializeField] Tilemap terrainTilemap;
    [SerializeField] List<Tile> terrainTiles = new();
    [SerializeField] GameObject pawnPrefab;
    [SerializeField] Transform cam;
    [SerializeField] short worldSize = 50;
    float[,] worldGrid;
    bool[,] blockedGrid;
    

    public short WorldSize => worldSize;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Application.targetFrameRate = gameFPS;

        blockedGrid = new bool[worldSize, worldSize];
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

    public void GeneratePawn(Vector2Int position, DirectionSpriteData bodySprite, DirectionSpriteData headSprite = null, DirectionSpriteData hairSprite = null)
    {
        if (pawnPrefab == null) return;
        GameObject spawned = Instantiate(pawnPrefab);
        Pawn pM = spawned.GetComponent<Pawn>();
        if (pM == null || bodySprite == null || headSprite == null) return;
        BodyData bmanager = pM.BodyData;
        if (bmanager) bmanager.SetDirectionSpriteData(bodySprite);
        HeadData hmanager = pM.HeadData;
        if (hmanager) hmanager.SetDirectionSpriteData(headSprite);
        HairData hairmanager = pM.HairData;
        if (hairmanager) hairmanager.SetDirectionSpriteData(hairSprite);
        spawned.transform.position = new Vector3(position.x, position.y, 0);
    }

    public void GeneratePawn(Vector2Int position, DirectionTexturePreset preset)
    {
        if (pawnPrefab == null) return;
        Pawn pM = Instantiate(pawnPrefab).GetComponent<Pawn>();
        if (pM == null || preset == null) return;
        pM.currentGridPosition = position;
        BodyData bmanager = pM.BodyData;
        if (bmanager) bmanager.SetDirectionSpriteData(preset.body);
        HeadData hmanager = pM.HeadData;
        if (hmanager) hmanager.SetDirectionSpriteData(preset.head);
        HairData hairmanager = pM.HairData;
        if (hairmanager) hairmanager.SetDirectionSpriteData(preset.hair);
        pM.transform.position = new Vector3(position.x, position.y, 0);
    }

    public void GenerateAttire(Pawn pawn)
    {

    }
}
