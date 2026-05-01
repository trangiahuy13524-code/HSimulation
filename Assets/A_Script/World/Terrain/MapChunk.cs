using UnityEngine;

public class MapChunk
{
    public Vector2Int chunkCoord;
    public Vector2Int size;
    public Map map;

    public MapMesh mesh;

    public MapChunk(Map map, Vector2Int chunkCoord, Vector2Int size)
    {
        this.map = map;
        this.chunkCoord = chunkCoord;
        this.size = size;

        Generate();
    }

    public void Generate()
    {
        mesh = new MapMesh(map, this);
    }

    public int StartX => chunkCoord.x * size.x;
    public int StartY => chunkCoord.y * size.y;
}