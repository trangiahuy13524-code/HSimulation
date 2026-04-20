using System;
using UnityEngine;

public class Wall : ObjectBase
{
    protected override bool isPassable => false;
    void OnDestroy()
    {
        if (World.Instance != null) World.Instance.SetWallTile(currentGridPos, null);
    }
}
