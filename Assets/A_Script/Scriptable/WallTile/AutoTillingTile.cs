using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable, CreateAssetMenu(fileName = "New AutoTilling Tile", menuName = "Tiles/AutoTilling Tile")]
public class AutoTillingTile : TileBase
{
    public Sprite[] sprites;

    public override void GetTileData(
        Vector3Int position,
        ITilemap tilemap,
        ref TileData tileData)
    {
        if (sprites != null && sprites.Length > 0) tileData.sprite = sprites[GetIndex(position, tilemap)];
        
    }

    private int GetIndex(Vector3Int position, ITilemap tilemap)
    {
        int index = 0;

        if (HasTile(position + Vector3Int.up, tilemap)) index |= 1;
        if (HasTile(position + Vector3Int.right, tilemap)) index |= 2;
        if (HasTile(position + Vector3Int.down, tilemap)) index |= 4;
        if (HasTile(position + Vector3Int.left, tilemap)) index |= 8;

        //if (HasSameTile(position + Vector3Int.up + Vector3Int.right, tilemap)) index |= 16;
        //if (HasSameTile(position + Vector3Int.down + Vector3Int.right, tilemap)) index |= 32;
        //if (HasSameTile(position + Vector3Int.down + Vector3Int.left, tilemap)) index |= 64;
        //if (HasSameTile(position + Vector3Int.up + Vector3Int.left, tilemap)) index |= 128;

        return index;
    }

    private bool HasTile(Vector3Int position, ITilemap tilemap)
    {
        return tilemap.GetTile(position) != null;
    }
}