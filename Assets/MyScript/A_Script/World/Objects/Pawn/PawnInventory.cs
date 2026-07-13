using System.Collections.Generic;
using UnityEngine;
using System;

public partial class Pawn
{
    [Header("Pawn Inventory")]
    Dictionary<ItemKey, int> inventory = new Dictionary<ItemKey, int>();
    [SerializeField] Item holdedItem;
    [SerializeField] Transform handTransform;

    public Item HoldedItem => holdedItem;
    public void TakeItemInventory(
    Item item,
    int amount)
    {
        ItemKey key = new(item.itemData, item.itemClass);

        if (inventory.ContainsKey(key))
        {
            inventory[key] += amount;
        }
        else
        {
            inventory[key] = amount;
        }

        item.ReduceStack(amount);
    }

    public int GetItemCountInventory(
    DataItem item,
    ItemClass itemClass)
    {
        ItemKey key = new(item, itemClass);

        if (inventory.TryGetValue(key, out int count))
        {
            return count;
        }

        return 0;
    }

    public (int, List<Item>) DropItemInventory(
    DataItem itemData,
    ItemClass itemClass,
    int amount,
    Vector2Int? dropPos, WorldObject reservingOb)
    {
        ItemKey key = new(itemData, itemClass);
        if (!inventory.ContainsKey(key))
            return (amount, null);

        int droppedAmount =
            Mathf.Min(inventory[key], amount);

        inventory[key] -= droppedAmount;

        if (inventory[key] <= 0)
        {
            inventory.Remove(key);
        }

        var items = world.CreateItem(dropPos ?? currentGridPos, itemData, itemClass, droppedAmount, reservingOb);

        // return remaining amount not dropped
        return (amount - droppedAmount, items);
    }

    public void RemoveItemsInventory(List<ItemDataContainer> requireItemDatas)
    {
        foreach (var req in requireItemDatas)
        {
            ItemKey key = new(req.itemData, req.itemClass);
            if (inventory.ContainsKey(key))
            {
                inventory[key] -= req.amount;
                if (inventory[key] <= 0)
                {
                    inventory.Remove(key);
                }
            }
        }
    }

    public void DropAllItemsInventory(Vector2Int? dropPos, WorldObject reservingOb)
    {
        foreach (var kvp in inventory)
        {
            ItemKey key = kvp.Key;
            int amount = kvp.Value;
            world.CreateItem(dropPos ?? currentGridPos, key.itemData, key.itemClass, amount, reservingOb);
        }
        inventory.Clear();
    }

    public int HoldItem(Item item, int amount)
    {
        if (item == null) return 0;

        if (holdedItem != null)
        {
            if (holdedItem.itemData == item.itemData && holdedItem.itemClass == item.itemClass)
            {
                holdedItem.SetLayer(WorldStatic.Instance.topGridLayer + 1);
                int takenAmount = amount;
                if (amount >= item.StackCount)
                {
                    takenAmount = item.StackCount;
                    holdedItem.StackCount += takenAmount;
                    item.Despawn();
                    return takenAmount;
                }
                else
                {
                    holdedItem.StackCount += takenAmount;
                    item.ReduceStack(takenAmount);
                    item.reservingObject = null;
                    return takenAmount;
                }
            }
            else
            {
                return 0;
            }
        }

        holdedItem = item;
        holdedItem.reservingObject = this;
        holdedItem.transform.SetParent(handTransform);
        holdedItem.transform.localPosition = Vector3.zero;
        holdedItem.SetLayer(WorldStatic.Instance.topGridLayer + 1);
        world.RegisterItem(null, holdedItem.CurrentGridPosition);

        return holdedItem.StackCount;
    }

    public List<Item> DropHoldedItem(WorldObject reservingOb)
    {
        if (holdedItem == null)
            return new List<Item>{};

        List<Item> items;  
        if (holdedItem.itemData.IsStackable)
        {
            items = world.CreateItem(currentGridPos, holdedItem.itemData, holdedItem.itemClass, holdedItem.StackCount, reservingOb);
            holdedItem.Despawn();
            holdedItem = null;
        }
        else
        {
            
            holdedItem.transform.SetParent(null);
            holdedItem.CurrentGridPosition = currentGridPos;
            holdedItem.reservingObject = reservingOb;
            world.RegisterItem(holdedItem, holdedItem.CurrentGridPosition);
            items = new()
            {
                holdedItem
            };
            holdedItem = null;
            
        }
        return items;
    }
}