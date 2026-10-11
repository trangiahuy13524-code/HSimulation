using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Wall : WorldObjectStatic
{
    //protected MapRenderer mapRenderer;
    public override bool isPassable => false;
    public DataWall wallData {  get; protected set; }
    public Tilemap wallTileMap { get; protected set; }
    public Tile tile {  get; protected set; }

    public void SetData(DataWall wallData, Tilemap tilemap)
    {
        //mapRenderer = MapRenderer.Instance;
        this.wallData = wallData;
        wallTileMap = tilemap;
    }

    public override void Initialize()
    {
        base.Initialize();
        SetWallTile(currentGridPos, wallData);
    }

    public override Sprite IconSprite { get { return wallData?.Icon; } }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        SetWallTile(currentGridPos, null);
        //if (mapRenderer != null) mapRenderer.map.RevertToTerrainTile(currentGridPos);
        if (world != null) world.ResetNotPassableGrid(currentGridPos);
    }

    private void SetWallTile(Vector2Int position, DataWall tile)
    {
        if (wallTileMap == null) return;
        if (!world.IsInside(position)) return;
        int x = position.x;
        int y = position.y;
        wallTileMap.SetTile(new Vector3Int(x, y, 0), tile);
        RefreshNeighborWall(x, y);
    }

    private void RefreshNeighborWall(int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                wallTileMap.RefreshTile(new Vector3Int(x + dx, y + dy, 0));
            }
        }
    }
}
