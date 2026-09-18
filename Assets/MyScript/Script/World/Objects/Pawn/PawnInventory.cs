using System.Collections.Generic;
using UnityEngine;

public partial class Pawn
{
    [Header("Pawn Inventory")]
    private readonly Dictionary<ItemKey, int> inventory = new();
    [SerializeField] Item holdedItem;
    [SerializeField] Transform handTransform;

    public Item HoldedItem => holdedItem;

    public void TakeItemInventory(Item item, int amount)
    {
        ItemKey key = new(item.itemData, item.itemClass);
        inventory.TryGetValue(key, out int currentAmount);
        inventory[key] = currentAmount + amount;
        item.ReduceStack(amount);
    }

    public int GetItemCountInventory(DataItem item, ItemClass itemClass)
    {
        ItemKey key = new(item, itemClass);
        return inventory.TryGetValue(key, out int count) ? count : 0;
    }

    public (int, List<Item>) DropItemInventory(
    DataItem itemData,
    ItemClass itemClass,
    int amount,
        Vector2Int? dropPos, WorldObject reservingOb)
    {
        ItemKey key = new(itemData, itemClass);
        if (!inventory.TryGetValue(key, out int storedAmount)) return (amount, null);

        int droppedAmount = Mathf.Min(storedAmount, amount);

        storedAmount -= droppedAmount;

        if (storedAmount <= 0)
        {
            inventory.Remove(key);
        }
        else
        {
            inventory[key] = storedAmount;
        }

        List<Item> items = world.CreateItem(dropPos ?? currentGridPos, itemData, itemClass, droppedAmount, reservingOb);
        return (amount - droppedAmount, items);
    }

    public void RemoveItemsInventory(List<ItemDataContainer> requireItemDatas)
    {
        foreach (ItemDataContainer requirement in requireItemDatas)
        {
            ItemKey key = new(requirement.itemData, requirement.itemClass);
            if (inventory.TryGetValue(key, out int storedAmount))
            {
                storedAmount -= requirement.amount;
                if (storedAmount <= 0)
                {
                    inventory.Remove(key);
                }
                else
                {
                    inventory[key] = storedAmount;
                }
            }
        }
    }

    public void DropAllItemsInventory(Vector2Int? dropPos, WorldObject reservingOb)
    {
        foreach (KeyValuePair<ItemKey, int> itemEntry in inventory)
        {
            ItemKey key = itemEntry.Key;
            int amount = itemEntry.Value;
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
                holdedItem.SetLayer(WorldData.Instance.topGridLayer + 1);
                if (amount >= item.StackCount)
                {
                    int takenAmount = item.StackCount;
                    holdedItem.StackCount += takenAmount;
                    item.Despawn();
                    return takenAmount;
                }

                holdedItem.StackCount += amount;
                item.ReduceStack(amount);
                item.reservingObject = null;
                return amount;
            }

            return 0;
        }

        holdedItem = item;
        holdedItem.reservingObject = this;
        holdedItem.transform.SetParent(handTransform);
        holdedItem.transform.localPosition = Vector3.zero;
        holdedItem.SetLayer(WorldData.Instance.topGridLayer + 1);
        world.RegisterItem(null, holdedItem.CurrentGridPosition);

        return holdedItem.StackCount;
    }

    public List<Item> DropHoldedItem(WorldObject reservingOb)
    {
        if (holdedItem == null)
            return new List<Item>();

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
