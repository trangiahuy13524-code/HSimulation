using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTile
{
    public TerrainType terrainType;
    public Vector2Int position;
    public Map map; // We will create the map class just after this.
    public CustomTile(Vector2Int position, TerrainType terrainType, Map map)
    {
        this.position = position;
        this.terrainType = terrainType;
        this.map = map;
    }
    public CustomTile(int x, int y, TerrainType terrainType, Map map): this(new Vector2Int(x, y), terrainType, map) { }
}

public class Map
{
    Dictionary<Vector2Int, TerrainType> oldTiles = new();
    public System.Action<CustomTile> OnTileChanged;
    public CustomTile[,] tiles;
    public Vector2Int size;
    public Map(Vector2Int size, float[,] noiseMap)
    {
        this.size = size;
        this.tiles = new CustomTile[this.size.x, this.size.y];

        for(int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < this.size.y; y++)
            {
                float value = noiseMap[x, y];
                this.tiles[x, y] = new CustomTile(x, y, Terrain.GetTerrain(value), this);
            }
        }
       
        Debug.Log("Map intialized with a size of" + this.size);
    }
    public CustomTile this[Vector2Int v2]
    {
        get
        {
            return this[v2.x, v2.y];
        }
    }
    public CustomTile this[int x, int y]
    {
        get
        {
            if (x >= 0 && y >= 0 && x < this.size.x && y < this.size.y)
            {
                return this.tiles[x, y];
            }
            return null;
        }
    }
    public void RevertToTerrainTile(Vector2Int position)
    {
        if (!oldTiles.TryGetValue(position, out var terrainTile))
            return;

        CustomTile tile = tiles[position.x, position.y];

        tile.terrainType = terrainTile;

        oldTiles.Remove(position); // IMPORTANT

        OnTileChanged?.Invoke(tile);
    }
    public void RevertToTerrainTile(int x, int y)
    {
        RevertToTerrainTile(new Vector2Int(x, y));
    }
    public void SetTile(int x, int y, PlaceableTile placingTile)
    {
        SetTile(new Vector2Int(x, y), placingTile);
    }
    public void SetTile(Vector2Int position, PlaceableTile placingTile)
    {
        CustomTile tile = this[position.x, position.y];

        if (tile == null)
            return;

        if (tile.terrainType.source == placingTile)
            return;

        oldTiles.TryAdd(position, tile.terrainType);

        tile.terrainType = TerrainType.Get(placingTile);

        OnTileChanged?.Invoke(tile);
    }
}