using System;
using UnityEngine;

public class Wall : WorldObject
{
    protected override bool isPassable => false;
    protected override void Start()
    {
        base.Start();
    }
    void OnDestroy()
    {
        if (world != null) world.SetWallTile(currentGridPos, null);
        if (mapRenderer != null) mapRenderer.map.RevertToTerrainTile(currentGridPos);
    }
}
