using System.Collections.Generic;
using UnityEngine;

public class MapRenderer : MonoBehaviour
{
    public static MapRenderer Instance;

    public Vector2Int chunkSize = new Vector2Int(15, 15);
    private Dictionary<Vector2Int, MapChunk> chunks = new Dictionary<Vector2Int, MapChunk>();
    public Map map;
    public bool ready = false;

    void Start()
    {
        Instance = this;
        P_Terrain terrain = P_Terrain.Instance;

        if (terrain == null)
        {
            Debug.LogError("No Terrain instance found.");
            return;
        }

        if (terrain.terrainData == null ||
            terrain.terrainData.terrainTypes == null ||
            terrain.terrainData.terrainTypes.Length == 0)
        {
            Debug.LogError("Terrain data missing.");
            return;
        }

        int size = WorldMap.Instance.WorldSize;

        map = new Map(
            new Vector2Int(size, size),
            terrain.noiseMap);

        CreateChunks();
        map.OnTileChanged += HandleTileChanged;
        ready = true;
    }

    void CreateChunks()
    {
        int chunkCountX = Mathf.CeilToInt((float)map.size.x / chunkSize.x);
        int chunkCountY = Mathf.CeilToInt((float)map.size.y / chunkSize.y);

        for (int cx = 0; cx < chunkCountX; cx++)
        {
            for (int cy = 0; cy < chunkCountY; cy++)
            {
                Vector2Int coord = new Vector2Int(cx, cy);

                MapChunk chunk = new MapChunk(map, coord, chunkSize);

                chunk.Generate(); // REQUIRED

                chunks.Add(coord, chunk);

                CreateChunkGameObjects(chunk);
            }
        }
    }

    void CreateChunkGameObjects(MapChunk chunk)
    {
        chunk.root = new GameObject($"Chunk {chunk.chunkCoord}");
        chunk.root.transform.SetParent(transform);

        chunk.root.transform.localPosition =
            new Vector3(chunk.StartX, chunk.StartY, 0);

        if (chunk.mesh == null)
        {
            Debug.LogError($"Chunk {chunk.chunkCoord} mesh is NULL");
            return;
        }

        foreach (var kv in chunk.mesh.meshes)
        {
            TerrainType terrainType = kv.Key;
            MeshData meshData = kv.Value;

            GameObject go =
                new GameObject(terrainType.ToString());

            go.transform.SetParent(chunk.root.transform);
            go.transform.localPosition =
                new Vector3(0, 0, terrainType.layer);

            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = meshData.mesh;

            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = terrainType.terrainMaterial;
        }
    }
    Vector2Int GetChunkCoord(Vector2Int tilePos)
    {
        return new Vector2Int(
            tilePos.x / chunkSize.x,
            tilePos.y / chunkSize.y);
    }
    void HandleTileChanged(CustomTile tile)
    {
        Vector2Int chunkCoord = GetChunkCoord(tile.position);

        RebuildChunk(chunkCoord);

        // IMPORTANT:
        // also rebuild neighbour chunks if tile is on border
        CheckNeighbourChunks(tile.position);
    }
    void RebuildChunk(Vector2Int coord)
    {
        if (!chunks.TryGetValue(coord, out MapChunk chunk))
            return;

        if (chunk.root != null)
            Destroy(chunk.root);

        chunk.Generate();

        CreateChunkGameObjects(chunk);
    }
    void CheckNeighbourChunks(Vector2Int pos)
    {
        Vector2Int[] dirs =
        {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

        foreach (var d in dirs)
        {
            Vector2Int neighbour = pos + d;

            Vector2Int coord = GetChunkCoord(neighbour);

            RebuildChunk(coord);
        }
    }
}