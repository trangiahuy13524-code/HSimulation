using UnityEngine;

public class WorldObject : MonoBehaviour
{
    [SerializeField] protected string objectName;
    [SerializeField] protected Sprite iconSprite;
    [SerializeField] protected Vector2Int currentGridPos;
    [SerializeField] protected Vector2Int oldGridPos;
    [SerializeField] protected World world;
    [SerializeField] protected MapRenderer mapRenderer;
    protected virtual Vector2 size => Vector2.one;
    protected virtual bool isPassable => true;
    public bool IsPassable => isPassable;

    public virtual string ObjectName
    {
        get
        {
            return objectName;
        }
        set
        {
            objectName = value;
        }
    }
    public Vector2Int CurrentGridPosition
    {
        get => currentGridPos;
        set
        {
            currentGridPos = value;
            transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);
        }
    }
    public Vector2Int OldGridPosition => oldGridPos;

    public Sprite IconSprite => iconSprite;
    public Vector2 Size => size;

    protected virtual void Awake()
    {
        world = World.Instance;
        mapRenderer = MapRenderer.Instance;
    }

    protected virtual void Start()
    {
        transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);
        world.RegisterObject(this, currentGridPos);
    }
}