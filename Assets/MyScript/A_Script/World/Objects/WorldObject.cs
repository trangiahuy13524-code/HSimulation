using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour
{
    [SerializeField] protected string objectName;
    [SerializeField] protected Sprite iconSprite;
    protected Vector2Int currentGridPos;
    protected Vector2Int oldGridPos;
    protected WorldMap world;
    protected JobManager jobManager;
    protected ResearchManager researchManager;
    protected WorldStatic worldStatic;
    public bool isSelected { get; private set; }

    public List<Ability> abilities = new();

    public virtual bool isPassable => true;
    public virtual bool canHoldItems => false;

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
    public virtual Vector2Int CurrentGridPosition
    {
        get => currentGridPos;
        set
        {
            currentGridPos = value;
            transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);
        }
    }

    public virtual Sprite IconSprite => iconSprite;

    protected void Awake()
    {
        world = WorldMap.Instance;
        jobManager = JobManager.Instance;
        researchManager = ResearchManager.Instance;
        worldStatic = WorldStatic.Instance;
    }

    protected virtual void Start()
    {
        transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);
    }

    public virtual void SetSelected(bool value, byte strength)
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
            ObjectSelector.Instance.DeselectObject();
        }
    }

    public virtual Vector2 GetWorldPos()
    {
        return WorldUtility.GridToWorld(currentGridPos);
    }

    public virtual Vector2Int GetMidGrid()
    {
        return currentGridPos;
    }
}