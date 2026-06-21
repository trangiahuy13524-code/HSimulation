using UnityEngine;

public class MapChunk
{
    public GameObject root;
    public Vector2Int chunkCoord;
    public Vector2Int size;
    public Map map;

    public MapMesh mesh;

    public MapChunk(Map map, Vector2Int coord, Vector2Int size)
    {
        this.map = map;
        this.chunkCoord = coord;
        this.size = size;
    }

    public void Generate()
    {
        mesh = new MapMesh(map, this);
    }
    public void Rebuild()
    {
        if (root != null)
            GameObject.Destroy(root);

        mesh = new MapMesh(map, this);

        root = new GameObject($"Chunk {chunkCoord}");
    }

    public int StartX => chunkCoord.x * size.x;
    public int StartY => chunkCoord.y * size.y;
}