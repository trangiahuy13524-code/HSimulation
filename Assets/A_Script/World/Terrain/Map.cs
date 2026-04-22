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
                this.tiles[x, y] = new CustomTile(x, y, TerrainData.GetTerrain(value), this);
                //this.SetRect(x, y, 1, 1, World.GetTerrain(value));
            }
        }
       
        Debug.Log("Map intialized with a size of" + this.size);
    }

    // We just set a rectangle to a terrainType value.
    public void SetRect(int startX, int startY, int width, int height, TerrainType terrainType)
    {
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                this[x, y].terrainType = terrainType;
            }
        }
    }

    public IEnumerator<CustomTile> GetEnumerator()
    {
        for (int x = 0; x < this.size.x; x++)
        {
            for (int y = 0; y < this.size.y; y++)
            {
                yield return this[x, y];
            }
        }
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
}