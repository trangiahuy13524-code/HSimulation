using UnityEngine;

public class BaseObject : MonoBehaviour
{
    [SerializeField] protected Sprite iconSprite;
    [SerializeField] protected Vector2Int currentGridPos;
    [SerializeField] protected World world;
    protected virtual Vector2 size => Vector2.one;
    protected virtual bool isPassable => true;
    public bool IsPassable => isPassable;

    public Vector2Int CurrentGridPosition
    {
        get => currentGridPos;
        set
        {
            currentGridPos = value;
            transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);
        }
    }

    public Sprite IconSprite => iconSprite;
    public Vector2 Size => size;

    protected virtual void Awake()
    {
        world = World.Instance;
    }

    protected virtual void Start()
    {
        transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);
        world.RegisterObject(this, currentGridPos);
    }
}
