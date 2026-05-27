using System.Collections.Generic;
using UnityEngine;
using System;

public partial class Pawn
{
    Dictionary<InventoryKey, int> inventory = new Dictionary<InventoryKey, int>();

    public void TakeItem(
    Item item,
    int amount)
    {
        InventoryKey key = new(item.itemData, item.itemClass);

        if (inventory.ContainsKey(key))
        {
            inventory[key] += amount;
        }
        else
        {
            inventory[key] = amount;
        }

        item.PickedUp(amount);
    }

    public int GetItemCount(
    ItemData item,
    ItemClass itemClass)
    {
        InventoryKey key = new(item, itemClass);

        if (inventory.TryGetValue(key, out int count))
        {
            return count;
        }

        return 0;
    }

    public (int, List<Item>) DropItem(
    ItemData itemData,
    ItemClass itemClass,
    int amount,
    Vector2Int? dropPos)
    {
        InventoryKey key = new(itemData, itemClass);
        if (!inventory.ContainsKey(key))
            return (amount, null);

        int droppedAmount =
            Mathf.Min(inventory[key], amount);

        inventory[key] -= droppedAmount;

        if (inventory[key] <= 0)
        {
            inventory.Remove(key);
        }

        var items = world.CreateItem(dropPos ?? currentGridPos, itemData, itemClass, droppedAmount);

        // return remaining amount not dropped
        return (amount - droppedAmount, items);
    }

    //public (int, List<Item>) DropItem(ItemData itemData, int amount)
    //{
    //    int remaining = amount;

    //    // copy keys to avoid modifying dictionary while iterating
    //    List<InventoryKey> keys = new(inventory.Keys);

    //    List<Item> items = null;

    //    foreach (InventoryKey key in keys)
    //    {
    //        if (key.itemData != itemData)
    //            continue;

    //        int available = inventory[key];

    //        int droppedAmount =
    //            Mathf.Min(available, remaining);

    //        inventory[key] -= droppedAmount;

    //        if (inventory[key] <= 0)
    //        {
    //            inventory.Remove(key);
    //        }

    //        items = world.CreateItem(currentGridPos, itemData, key.itemClass, droppedAmount);

    //        remaining -= droppedAmount;

    //        if (remaining <= 0)
    //            break;
    //    }

    //    return (remaining, items);
    //}
}

[Serializable]
public struct InventoryKey
{
    public ItemData itemData;
    public ItemClass itemClass;

    public InventoryKey(ItemData data, ItemClass itemClass)
    {
        this.itemData = data;
        this.itemClass = itemClass;
    }

    public override bool Equals(object obj)
    {
        if (obj is not InventoryKey other)
            return false;

        return itemData == other.itemData &&
               itemClass == other.itemClass;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(itemData, itemClass);
    }
}