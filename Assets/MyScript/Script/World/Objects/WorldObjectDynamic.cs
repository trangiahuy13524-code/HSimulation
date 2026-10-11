using UnityEngine;

public class WorldObjectDynamic : WorldObject
{
    public override Vector2Int CurrentGridPosition
    {
        get => currentGridPos;
        set
        {
            currentGridPos = value;
            transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);
            UpdateLayer();
        }
    }

    public virtual void UpdateLayer()
    {
        
    }
}