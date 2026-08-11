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

    protected override async UniTask<ActionResult> WorkRoutine(
        Pawn worker,
        WorkPosition workPos,
        CancellationToken token)
    {
        if (unfinishedItem == null)
        {
            var collectResult =
                await CollectIngredients(worker, workPos, token);

            if (collectResult != ActionResult.Success)
                return collectResult;

            if (!ValidateIngredients())
                return ActionResult.Cancelled;

            ConsumeIngredients();

            unfinishedItem = SpawnUnfinishedItem(worker);
        }
        else
        {
            var carryResult =
                await CarryUnfinishedItem(worker, workPos, token);

            if (carryResult != ActionResult.Success)
                return carryResult;
        }

        return await worker.DoProgressWork(
            workBuilding.workDirection,
            token);
    }

    // =====================================================
    // COLLECT INGREDIENTS
    // =====================================================

    private async UniTask<ActionResult> CollectIngredients(
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
                var result =
                    await PickRequiredItem(worker, workPos, required, remaining, token);

                if (result.result != ActionResult.Success)
                    return result.result;

                remaining -= result.amount;

                while (remaining > 0 &&
                       worker.HoldedItem != null &&
                       worker.HoldedItem.StackCount <
                       worker.HoldedItem.itemData.maxStack)
                {
                    result =
                        await PickRequiredItem(worker, workPos, required, remaining, token);

                    if (result.result != ActionResult.Success)
                        return result.result;

                    remaining -= result.amount;
                }

                var dropResult =
                    await DropHeldItems(worker, workPos, token);

                if (dropResult != ActionResult.Success)
                    return dropResult;
            }
        }

        return ActionResult.Success;
    }

    private async UniTask<(ActionResult result, int amount)> PickRequiredItem(
        Pawn worker,
        WorkPosition workPos,
        ItemDataContainer required,
        int remaining,
        CancellationToken token)
    {
        ItemContainer found =
            FindItem(workPos.workPos, required);

        if (found.item == null)
            return (ActionResult.Cancelled, 0);

        return await worker.MoveToAndPickUp(
            found.item,
            remaining,
            token);
    }

    private async UniTask<ActionResult> DropHeldItems(
        Pawn worker,
        WorkPosition workPos,
        CancellationToken token)
    {
        var result =
            await worker.MoveTo(workPos.workPos, token);

        if (result != ActionResult.Success)
            return result;

        worker.ChangeDirection(workBuilding.workDirection);

        await UniTask.Yield(token);

        usedItems.UnionWith(worker.DropHoldedItem(worker));

        return ActionResult.Success;
    }

    // =====================================================
    // VALIDATE + CONSUME
    // =====================================================

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

    // =====================================================
    // UNFINISHED ITEM
    // =====================================================

    private Item SpawnUnfinishedItem(Pawn worker)
    {
        return WorldMap.Instance.CreateItem(
            worker.CurrentGridPosition + worker.direction(),
            recipeData.unfinishedCraftItemData,
            ItemClass.None,
            1,
            worker)[0];
    }

    private async UniTask<ActionResult> CarryUnfinishedItem(
        Pawn worker,
        WorkPosition workPos,
        CancellationToken token)
    {
        var result =
            await worker.MoveToAndPickUp(
                unfinishedItem,
                1,
                token);

        if (result.Item1 != ActionResult.Success)
            return result.Item1;

        return await DropHeldItems(worker, workPos, token);
    }

    // =====================================================
    // FINISH / RETURN
    // =====================================================

    public override void ReturnJob(Pawn worker)
    {
        if (unfinishedItem != null)
            unfinishedItem.reservingObject = null;

        foreach (var item in usedItems)
        {
            if (item != null)
                item.reservingObject = null;
        }

        worker.DropHoldedItem(null);

        base.ReturnJob(worker);
    }

    protected override async UniTask FinishWork(
        Pawn worker,
        CancellationToken token)
    {
        if (unfinishedItem != null)
            unfinishedItem.Despawn();

        await UniTask.Yield(token);
        foreach (var output in recipeData.outputItems)
        {
            WorldMap.Instance.CreateItem(
                worker.CurrentGridPosition + worker.direction(),
                output.itemData,
                output.itemClass,
                output.amount,
                null);

            await UniTask.Yield(token);
        }

        await base.FinishWork(worker, token);
    }

    // =====================================================
    // FIND ITEM
    // =====================================================

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