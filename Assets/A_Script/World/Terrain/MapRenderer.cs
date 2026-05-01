using System.Collections.Generic;
using UnityEngine;

public class MapRenderer : MonoBehaviour
{
    public Vector2Int chunkSize = new Vector2Int(32, 32);
    private Dictionary<Vector2Int, MapChunk> chunks = new Dictionary<Vector2Int, MapChunk>();
    public Map map;
    public bool ready = false;

    void Start()
    {
        Terrain terrain = Terrain.Instance;

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

        int size = World.Instance.WorldSize;

        map = new Map(
            new Vector2Int(size, size),
            terrain.noiseMap);

        CreateChunks();

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

                MapChunk chunk =
                    new MapChunk(map, coord, chunkSize);

                chunks.Add(coord, chunk);

                CreateChunkGameObjects(chunk);
            }
        }
    }

    void CreateChunkGameObjects(MapChunk chunk)
    {
        foreach (var kv in chunk.mesh.meshes)
        {
            TerrainType terrainType = kv.Key;
            MeshData meshData = kv.Value;

            GameObject go =
                new GameObject($"Chunk {chunk.chunkCoord} - {terrainType}");

            go.transform.SetParent(transform);

            go.transform.localPosition =
                new Vector3(
                    chunk.StartX,
                    chunk.StartY,
                    terrainType.layer);

            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = meshData.mesh;

            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = terrainType.terrainMaterial;
        }
    }
}