using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainBiome : ScriptableObject
{
    public TerrainType[] terrainTypes;

    void OnValidate()
    {
        if (terrainTypes == null || terrainTypes.Length == 0)
            return;

        // Sort by height
        for (int i = 0; i < terrainTypes.Length - 1; i++)
        {
            TerrainType current = terrainTypes[i];
            TerrainType next = terrainTypes[i + 1];

            if (current.heightThreshold > next.heightThreshold)
                next.heightThreshold = current.heightThreshold;
        }

        // Assign layers
        //for (int i = 0; i < terrainTypes.Length; i++)
        //{
        //    terrainTypes[i].layer = (byte)i;
        //}

        // Force last terrain to height = 1
        terrainTypes[^1].heightThreshold = 1f;
    }
}

[Serializable]
public class TerrainType
{
    public string terrainName;
    [Range(0, 1f)] public float heightThreshold;
    public Material terrainMaterial;
    public bool blend = true;
    public short movementCost;
    public byte layer;

    public DataTile source { get; set; }

    // SHARED INSTANCE CACHE
    static Dictionary<DataTile, TerrainType> cache = new();

    private TerrainType(DataTile tile)
    {
        terrainName = tile.thingName;
        terrainMaterial = tile.terrainMaterial;
        blend = tile.blend;
        movementCost = tile.movementCost;
        layer = tile.layer;
        source = tile;
    }

    // FACTORY METHOD
    public static TerrainType Get(DataTile tile)
    {
        if (tile == null)
            return null;

        if (!cache.TryGetValue(tile, out var terrain))
        {
            terrain = new TerrainType(tile);
            cache.Add(tile, terrain);
        }

        return terrain;
    }
}