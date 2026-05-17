using UnityEngine;

public class Item : WorldObject
{
    protected override bool isPassable => true;
    [SerializeField] ItemData itemData;
    [SerializeField] bool initialized = false;
    [SerializeField] ItemClass itemClass;
    public void InitializeItem(ItemData data)
    {
        if (initialized) return;
        itemData = data;
        iconSprite = data.icon;
        objectName = data.itemName;
        initialized = true;
    }
}