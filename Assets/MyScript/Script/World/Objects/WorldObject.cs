using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour
{
    public virtual string ThingName => null;
    public virtual Sprite IconSprite => null;
    protected Vector2Int currentGridPos;
    protected Vector2Int oldGridPos;
    protected WorldMap world;
    protected JobManager jobManager;
    protected ResearchManager researchManager;
    protected WorldData worldData;
    public bool isSelected { get; private set; }

    public List<DataAbility> abilities = new();

    public virtual bool isPassable => true;
    public virtual bool canHoldItems => false;
    public virtual Vector2Int CurrentGridPosition
    {
        get => currentGridPos;
        set
        {
            currentGridPos = value;
            transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);
        }
    }

    

    protected void Awake()
    {
        world = WorldMap.Instance;
        jobManager = JobManager.Instance;
        researchManager = ResearchManager.Instance;
        worldData = WorldData.Instance;
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