using UnityEngine;

public class WorldObjectDynamic : WorldObject
{
    public override Vector2Int CurrentGridPosition
    {
        get => currentGridPos;
        set
        {
            base.CurrentGridPosition = value;
            UpdateLayer();
        }
    }

    public virtual void UpdateLayer()
    {
        
    }
}