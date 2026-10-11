using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class JobCraft : JobBuilding
{
    public RecipeData recipeData;

    private Item unfinishedItem;
    private readonly HashSet<Item> usedItems = new();

    public BuildingCraft CraftBuilding => (BuildingCraft)workBuilding;

    private const int UNFINISHED_ITEM_AMOUNT = 1;

    protected override async UniTask WorkRoutine(
        Pawn worker,
        WorkPosition workPos,
        CancellationToken token)
    {
        if (unfinishedItem == null)
        {
            await CollectIngredients(worker, workPos, token);
            token.ThrowIfCancellationRequested();

            if (!ValidateIngredients())
            {
                throw new OperationCanceledException(token);
            }

            ConsumeIngredients();
            unfinishedItem = SpawnUnfinishedItem(worker);
        }
        else
        {
            await CarryUnfinishedItem(worker, workPos, token);
        }

        await worker.DoProgressWork(workBuilding.workDirection, token);
    }

    private async UniTask CollectIngredients(
        Pawn worker,
        WorkPosition workPos,
        CancellationToken token)
    {
        currentProgress = 0;

        foreach (var required in recipeData.requiredItems)
        {
            int remaining = required.amount;
            while (remaining > 0)
            {
                remaining -= await PickRequiredItem(worker, workPos, required, remaining, token);

                while (remaining > 0 &&
                       worker.HoldedItem != null &&
                       worker.HoldedItem.StackCount < worker.HoldedItem.itemData.maxStack)
                {
                    remaining -= await PickRequiredItem(worker, workPos, required, remaining, token);
                }

                await DropHeldItems(worker, workPos, token);
            }
        }
    }

    private async UniTask<int> PickRequiredItem(
        Pawn worker,
        WorkPosition workPos,
        ItemDataContainer required,
        int remaining,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        ItemContainer found = FindItem(workPos.workPos, required);
        if (found.item == null)
        {
            throw new OperationCanceledException(token);
        }

        int amount = await worker.MoveToAndPickUp(found.item, remaining, token);
        if (amount <= 0)
        {
            throw new OperationCanceledException(token);
        }

        return amount;
    }

    private async UniTask DropHeldItems(
        Pawn worker,
        WorkPosition workPos,
        CancellationToken token)
    {
        await worker.MoveTo(workPos.workPos, token);
        worker.ChangeDirection(workBuilding.workDirection);
        token.ThrowIfCancellationRequested();
        usedItems.UnionWith(worker.DropHoldedItem(worker));
    }
    private bool ValidateIngredients()
    {
        Dictionary<ItemKey, int> requiredMap =
            BuildRequiredMap();

        foreach (var item in usedItems)
        {
            ItemKey key =
                new(item.itemData, item.itemClass);

            if (!requiredMap.ContainsKey(key))
                continue;

            requiredMap[key] -= item.StackCount;

            if (requiredMap[key] <= 0)
                requiredMap.Remove(key);
        }

        return requiredMap.Count == 0;
    }

    private void ConsumeIngredients()
    {
        Dictionary<ItemKey, int> consumeMap =
            BuildRequiredMap();

        foreach (var item in usedItems)
        {
            if (item == null) continue;

            ItemKey key =
                new(item.itemData, item.itemClass);

            if (!consumeMap.ContainsKey(key))
                continue;

            int need = consumeMap[key];

            if (item.StackCount > need)
            {
                item.ReduceStack(need);
                consumeMap.Remove(key);
            }
            else
            {
                consumeMap[key] -= item.StackCount;
                item.Despawn();

                if (consumeMap[key] <= 0)
                    consumeMap.Remove(key);
            }
        }
    }

    private Dictionary<ItemKey, int> BuildRequiredMap()
    {
        Dictionary<ItemKey, int> map = new();

        foreach (var item in recipeData.requiredItems)
        {
            ItemKey key =
                new(item.itemData, item.itemClass);

            if (!map.ContainsKey(key))
                map[key] = 0;

            map[key] += item.amount;
        }

        return map;
    }

    private Item SpawnUnfinishedItem(Pawn worker)
    {
        return WorldMap.Instance.CreateItem(
            worker.CurrentGridPosition + worker.direction(),
            recipeData.unfinishedCraftItemData,
            ItemClass.None,
            UNFINISHED_ITEM_AMOUNT,
            worker)[0];
    }

    private async UniTask CarryUnfinishedItem(
        Pawn worker,
        WorkPosition workPos,
        CancellationToken token)
    {
        int amount = await worker.MoveToAndPickUp(unfinishedItem, UNFINISHED_ITEM_AMOUNT, token);
        if (amount <= 0)
        {
            throw new OperationCanceledException(token);
        }

        await DropHeldItems(worker, workPos, token);
    }
    public override void ReturnJob(Pawn worker)
    {
        if (unfinishedItem != null)
            unfinishedItem.reservingObject = null;

        foreach (var item in usedItems)
        {
            if (item != null)
                item.reservingObject = null;
        }

        usedItems.Clear();
        worker.DropHoldedItem(null);

        base.ReturnJob(worker);
    }

    protected override async UniTask FinishWork(Pawn worker, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        // Commit the completed craft without yielding between its outputs.
        foreach (var output in recipeData.outputItems)
        {
            WorldMap.Instance.CreateItem(
                worker.CurrentGridPosition + worker.direction(),
                output.itemData,
                output.itemClass,
                output.amount,
                null);
        }

        if (unfinishedItem != null)
        {
            unfinishedItem.Despawn();
        }

        await base.FinishWork(worker, token);
    }
    public ItemContainer FindItem(
        Vector2Int workPos,
        ItemDataContainer required)
    {
        Item item = WorldMap.Instance.FindNearestItem(
            required.itemData,
            required.itemClass,
            workPos);

        if (item == null)
            return default;

        return new ItemContainer
        {
            item = item,
            amount = required.amount
        };
    }
}


[Serializable]
public struct ItemDataContainer
{
    public DataItem itemData;
    public ItemClass itemClass;
    public int amount;
}

public struct ItemContainer
{
    public Item item;
    public int amount;
}
