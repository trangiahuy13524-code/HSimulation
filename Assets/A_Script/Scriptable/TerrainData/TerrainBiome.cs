using System;
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
        for (int i = 0; i < terrainTypes.Length; i++)
        {
            terrainTypes[i].layer = (byte)i;
        }

        // Force last terrain to height = 1
        terrainTypes[^1].heightThreshold = 1f;
    }
}

[Serializable]
public class TerrainType
{
    public string name;
    [Range(0, 1f)] public float heightThreshold;
    public Material terrainMaterial;
    public short movementCost;
    public byte layer;
}