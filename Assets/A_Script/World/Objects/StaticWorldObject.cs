using UnityEngine;

public class StaticWorldObject : WorldObject
{
    protected override void Start()
    {
        base.Start();
        world.RegisterObject(this, currentGridPos);
    }
}
