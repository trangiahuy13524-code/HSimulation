using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class Item : WorldObject
{
    public override bool isPassable => true;
    public ItemData itemData { get; private set; }
    public bool reserved = false;
    [SerializeField] int stackCount = 1;
    public int StackCount
    {
        get => stackCount;
        set
        {
            if (!itemData.isStackable)
            {
                stackCount = 1;
            }
            else
            {
                stackCount = value;
            }

            displayAmount.text = stackCount > 1
                ? stackCount.ToString()
                : "";
        }
    }
    [SerializeField] SpriteRenderer objectSpriteRenderder;
    [SerializeField] TextMeshPro displayAmount;
    public ItemClass itemClass;
    public void SetItemData(ItemData data, ItemClass itemClass, int amount = 1)
    {
        itemData = data;
        this.itemClass = itemClass;
        iconSprite = data.icon;
        objectName = data.itemName;
        objectSpriteRenderder.sprite = data.itemSprite.sprite;
        objectSpriteRenderder.size = data.itemSprite.size;
        transform.localPosition += (Vector3)data.itemSprite.offset;
        displayAmount.sortingOrder = 2000;
        StackCount = amount;
    }

    public int PickedUp(int amount)
    {
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
        UpdateLayer(world.WorldSize);
    }

    protected override void OnDestroy()
    {
        world.RemoveItem(currentGridPos);
        base.OnDestroy();
    }

    public virtual void UpdateLayer(int worldSize)
    {
        objectSpriteRenderder.sortingOrder = (worldSize - 1) * 5 - currentGridPos.y * 5 + 11;
    }
}