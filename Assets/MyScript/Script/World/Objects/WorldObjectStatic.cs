using UnityEngine;

public class WorldObjectStatic : WorldObject
{
    public override void Initialize()
    {
        base.Initialize();
        world.RegisterObject(this, currentGridPos);
    }
}
