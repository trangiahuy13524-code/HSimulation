using System;
using UnityEngine;

public class Wall : StaticWorldObject
{
    [SerializeField] protected MapRenderer mapRenderer;
    public override bool isPassable => false;
    protected override void Start()
    {
        base.Start();
        mapRenderer = MapRenderer.Instance;
    }
    protected override void OnDestroy()
    {
        if (world != null) world.SetWallTile(currentGridPos, null);
        if (mapRenderer != null) mapRenderer.map.RevertToTerrainTile(currentGridPos);
        if (world != null) world.ResetNotPassableGrid(currentGridPos);
    }
}
