using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Wall : WorldObjectStatic
{
    [SerializeField] protected MapRenderer mapRenderer;
    public override bool isPassable => false;
    public DataWall wallData {  get; protected set; }
    

    public void SetData(DataWall wallData)
    {
        this.wallData = wallData;
    }
    public override Sprite IconSprite { get { return wallData?.Icon; } }
    protected override void Start()
    {
        base.Start();
        mapRenderer = MapRenderer.Instance;
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (world != null) world.SetWallTile(currentGridPos, null);
        if (mapRenderer != null) mapRenderer.map.RevertToTerrainTile(currentGridPos);
        if (world != null) world.ResetNotPassableGrid(currentGridPos);
    }

    
}
