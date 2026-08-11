using UnityEngine;

[CreateAssetMenu]
public class DataTile : DataMain
{
    [Header("Tile")]
    public Material terrainMaterial;
    public bool blend = false;
    public short movementCost;
    public byte layer;
}
