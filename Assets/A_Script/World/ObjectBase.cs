using UnityEngine;

public class ObjectBase : MonoBehaviour
{
    public Sprite Icon;
    public Vector2Int currentGridPosition;

    protected virtual void Start()
    {
        transform.position = new Vector3(currentGridPosition.x, currentGridPosition.y, 0);
    }
}
