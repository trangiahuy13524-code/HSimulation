using System.Collections.Generic;
using UnityEngine;

public class MapRenderer : MonoBehaviour
{
    public Map map;
    public bool ready = false;

    void Start()
    {
        Terrain terrain = Terrain.Instance;
        if (terrain == null)
        {
            Debug.LogError("No Terrain instance found in the scene. Please add a Terrain component to a GameObject.");
            return;
        }
        if (terrain.terrainData == null || terrain.terrainData.terrainTypes == null || terrain.terrainData.terrainTypes.Length == 0)
        {
            Debug.LogError("Terrain instance does not have terrainData assigned. Please assign a TerrainBiome to the Terrain component.");
            return;
        }
        int size = World.Instance.WorldSize;
        this.map = new Map(new Vector2Int(size, size), terrain.noiseMap);

        MapMesh mapMesh = new MapMesh(this.map);

        foreach (KeyValuePair<TerrainType, MeshData> kv in mapMesh.meshes)
        {
            MeshData meshData = kv.Value; // It's just easier to read, you don't need to do this.
            TerrainType terrainType = kv.Key; // It's just easier to read, you don't need to do this.
            GameObject go = new GameObject("Mesh for " + terrainType.ToString());
            go.transform.SetParent(this.transform);

            // In our TerrainType enum Water=0, Dirt=1, Grass=2, Rocks=3
            // We always want to draw Rocks over Grass, Grass over Dirt, Dirt over Water
            // So we can just use the negative integer value as the Z position for the GameObject.
            go.transform.localPosition = new Vector3(0, 0, terrainType.layer);

            // Add a mesh filter and set the mesh to our mesh.
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = meshData.mesh;

            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = terrainType.terrainMaterial;
        }

        this.ready = true;
    }
}