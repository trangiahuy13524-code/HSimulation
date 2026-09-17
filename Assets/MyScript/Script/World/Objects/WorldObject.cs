using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour
{
    public virtual string ThingName => null;
    public virtual Sprite IconSprite { get; set; }
    protected Vector2Int currentGridPos;
    protected Vector2Int oldGridPos;
    protected WorldMap world;
    protected JobManager jobManager;
    protected ResearchManager researchManager;
    protected WorldData worldData;
    public bool isSelected { get; private set; }

    public List<DataAbility> fixedAbilities = new();
    public List<DataAbility> dynamicAbilities = new();

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

    [SerializeField] protected SpriteRenderer selectionSR;
    protected byte selectThreshHold = 0;
    public virtual void SetSelectedThreshold(bool value, byte strength)
    {
        if (value)
        {
            selectThreshHold += strength;
        }
        else
        {
            selectThreshHold -= strength;
        }
        if (selectThreshHold > 0)
        {
            if (selectionSR == null)
            {
                selectionSR = Instantiate(WorldData.Instance.selectionPrefab, transform);
                selectionSR.transform.localPosition = Vector3.down*0.5f;
            }
            else
            {
                selectionSR.gameObject.SetActive(true);
            }
            if (selectThreshHold == 1)
            {
                selectionSR.sprite = WorldData.Instance.hlSprite;
                isSelected = false;
            }
            else
            {
                selectionSR.sprite = WorldData.Instance.selSprite;
                isSelected = true;
            }
        }
        else
        {
            isSelected = false;
            if (selectionSR != null)
            {
                selectionSR.gameObject.SetActive(false);
            }
        }
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