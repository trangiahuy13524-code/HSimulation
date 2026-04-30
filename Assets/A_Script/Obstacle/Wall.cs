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
        if (world == null) world = World.Instance;
        world.SetWallTile(currentGridPos, null);
    }
}
