using UnityEngine;

public class ObjectBase : MonoBehaviour
{
    public Sprite Icon;
    public Vector2Int currentGridPos;

    protected virtual void Start()
    {
        transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);
    }
}
