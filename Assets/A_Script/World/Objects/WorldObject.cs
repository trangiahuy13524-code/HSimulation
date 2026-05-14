using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour
{
    [SerializeField] protected string objectName;
    [SerializeField] protected Sprite iconSprite;
    [SerializeField] protected Vector2Int currentGridPos;
    [SerializeField] protected Vector2Int oldGridPos;
    [SerializeField] protected World world;
    [SerializeField] protected JobManager jobManager;
    public bool isSelected { get; private set; }

    public List<Ability> abilities = new();

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

    public virtual Sprite IconSprite => iconSprite;

    protected virtual void Awake()
    {
        world = World.Instance;
        jobManager = JobManager.Instance;
    }

    protected virtual void Start()
    {
        transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);
        world.RegisterObject(this, currentGridPos);
    }

    public virtual void SetSelected(bool value)
    {
        isSelected = value;
    }

    public virtual void Despawn()
    {
        Destroy(gameObject);
    }

    protected virtual void OnDestroy()
    {
        if (isSelected)
        {
            ScreenAndTouchManager.Instance.DeselectObject();
        }
    }

    public virtual Vector3 GetWorldPos()
    {
        return WorldUtility.GridToWorld(currentGridPos);
    }
}