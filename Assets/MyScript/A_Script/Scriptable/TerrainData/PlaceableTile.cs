using UnityEngine;

[CreateAssetMenu]
public class PlaceableTile : ScriptableObject
{
    public string tileName;
    public Material terrainMaterial;
    public bool blend = false;
    public short movementCost;
    public byte layer;
}
