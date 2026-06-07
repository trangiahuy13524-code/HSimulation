using UnityEngine;

public class WorldObjectStatic : WorldObject
{
    protected override void Start()
    {
        base.Start();
        world.RegisterObject(this, currentGridPos);
    }
}
