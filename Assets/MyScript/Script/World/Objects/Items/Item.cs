using TMPro;
using UnityEngine;

public class Item : WorldObjectDynamic
{
    public override bool isPassable => true;
    public DataItem itemData { get; private set; }
    public bool reserved => reservingObject != null;
    public WorldObject reservingObject;
    [SerializeField] int stackCount = 1;
    public override string ThingName => itemData?.thingName;
    public override Sprite IconSprite => itemData?.icon;
    public int StackCount
    {
        get => stackCount;
        set
        {
            if (!itemData.IsStackable)
            {
                stackCount = 1;
            }
            else
            {
                stackCount = value;
            }

            displayAmount.SetText(stackCount > 1
                ? stackCount.ToString()
                : "");
        }
    }
    [SerializeField] SpriteRenderer objectSpriteRenderder;
    [SerializeField] TextMeshPro displayAmount;
    public ItemClass itemClass;
    public void SetItemData(DataItem data, ItemClass itemClass, int amount = 1)
    {
        itemData = data;
        this.itemClass = itemClass;
        objectSpriteRenderder.sprite = data.itemSprite.sprite;
        objectSpriteRenderder.size = data.itemSprite.size;
        transform.localPosition += (Vector3)data.itemSprite.offset;
        displayAmount.sortingOrder = 2000;
        StackCount = amount;
    }

    public int ReduceStack(int amount)
    {
        if (!itemData.IsStackable && amount > 0)
        {
            Despawn();
            return 1;
        }
        StackCount -= amount;
        if (StackCount <= 0)
        {
            Despawn();
            return amount + StackCount;
        }
        else
        {
            return amount;
        }
    }

    protected override void Start()
    {
        transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);
        world.RegisterItem(this, currentGridPos);
        UpdateLayer();
    }
    
    // protected override void OnDestroy()
    // {
    //     if (dataRemove) world.RemoveItem(currentGridPos);
    //     base.OnDestroy();
    // }

    public override void UpdateLayer()
    {
        if (worldData) objectSpriteRenderder.sortingOrder = worldData.topGridLayer - currentGridPos.y * worldData.spacing + 1;
    }

    public void SetLayer(int layer)
    {
        objectSpriteRenderder.sortingOrder = layer;
    }
}