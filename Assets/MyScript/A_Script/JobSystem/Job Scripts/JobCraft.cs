using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class JobCraft : JobBuilding
{
    public RecipeData recipeData;
    private Item onCraftItem;
    public ItemContainer itemFound;
    public BuildingCraft craftBuilding => (BuildingCraft)workBuilding;

    private List<ItemDataContainer> pickedUpItems = new List<ItemDataContainer>();
    protected override async UniTask<ActionResult> WorkRoutine(
    Pawn pawn,
    WorkPosition wP,
    CancellationToken token)
    {
        ActionResult result = ActionResult.Success;
        // =========================================
        // COLLECT ALL REQUIRED ITEMS
        // =========================================
        bool hasOnCraftItem = false;
        int remainingNeeded = 0;

        if (onCraftItem == null)
        {
            currentProgress = 0;
            bool first = true;
            for (int i = 0; i < recipeData.requiredItems.Count; i++)
            {
                ItemDataContainer required = recipeData.requiredItems[i];

                // how many still needed
                remainingNeeded =
                    required.amount -
                    pawn.GetItemCount(
                        required.itemData,
                        required.itemClass);

                // already enough
                if (remainingNeeded <= 0)
                    continue;

                // keep collecting until enough
                while (remainingNeeded > 0)
                {
                    token.ThrowIfCancellationRequested();

                    if (!first)
                    {
                        itemFound = await FindItem(pawn, wP.workPos, recipeData.requiredItems);
                        if (itemFound.item == null)
                        {
                            Debug.Log("Item not found, cancelling job");
                            return ActionResult.Cancelled;
                        }
                    }
                    else
                    {
                        first = false;
                        if (itemFound.item == null)
                        {
                            continue;
                        }
                    }


                    int pickupAmount = remainingNeeded;


                    (result, pickupAmount) = await pawn.MoveToAndPickUp(
                            itemFound.item,
                            pickupAmount,
                            token);

                    if (result != ActionResult.Success)
                    {

                        return result;
                    }

                    remainingNeeded -= pickupAmount;

                    await UniTask.Delay(100, cancellationToken: token);

                }
            }
        }
        else
        {
            onCraftItem.reserved = true;
            (result, remainingNeeded) = await pawn.MoveToAndPickUp(
                            onCraftItem,
                            1,
                            token);

            Debug.Log(onCraftItem);

            if (result != ActionResult.Success)
            {
                return result;
            }

            hasOnCraftItem = true;
        }

        // =========================================
        // DROP ALL REQUIRED ITEMS
        // =========================================

        //ClearReservedItems();

        result = await pawn.MoveTo(wP.workPos, token);

        if (result != ActionResult.Success)
        {
            Debug.Log("Failed to move to work position, cancelling job");
            return result;
        }


        pawn.ChangeDirection(workBuilding.workDirection);
        await UniTask.Yield(token);

        if (!hasOnCraftItem)
        {
            pawn.RemoveItems(recipeData.requiredItems);
            onCraftItem = World.Instance.CreateItem(
                pawn.CurrentGridPosition + pawn.direction(),
                recipeData.unfinishedCraftItemData,
                ItemClass.None,
                1)[0];
        }
        else
        {
            List<Item> droppedItems;
            (result, droppedItems) = await pawn.TryDrop(recipeData.unfinishedCraftItemData, ItemClass.None, 1, token);

            if (result != ActionResult.Success)
            {
                return result;
            }
            onCraftItem = droppedItems[0];
        }
        onCraftItem.reserved = true;
        //for (int i = 0; i < requiredItemDatas.Count; i++)
        //{
        //    RequireItemData required =
        //        requiredItemDatas[i];

        //    int amountInInventory =
        //        pawn.GetItemCount(
        //            required.itemData,
        //            required.itemClass);

        //    if (amountInInventory <= 0)
        //        continue;

        //    List<Item> droppedItems;
        //    (result, droppedItems) =
        //        await pawn.TryDrop(
        //            required.itemData,
        //            required.itemClass,
        //            Mathf.Min(required.amount, amountInInventory),
        //            token);

        //    foreach (var item in droppedItems)
        //    {
        //        AddReservedItem(item);
        //    }

        //    if (result != ActionResult.Success)
        //    {
        //        Debug.Log("Failed to drop item, cancelling job");
        //        return result;
        //    }

        //    await UniTask.Delay(100, cancellationToken: token);
        //}

        // =========================================
        // DO WORK
        // =========================================

        return await pawn.DoProgressWork(
            workBuilding.workDirection,
            token);
    }

    public override void ReturnJob()
    {
        if (onCraftItem != null)
        {
            onCraftItem.reserved = false;
        }
        base.ReturnJob();
    }

    protected override async UniTask FinishWork(Pawn pawn, CancellationToken token)
    {
        if (onCraftItem != null)
        {
            onCraftItem.Despawn();
        }
        await UniTask.Yield(token);
        foreach (var outputItemData in recipeData.outputItems)
        {
            World.Instance.CreateItem(
            pawn.CurrentGridPosition + pawn.direction(),
            outputItemData.itemData,
            outputItemData.itemClass,
            outputItemData.amount);
            await UniTask.Yield(token);
        }
        await base.FinishWork(pawn, token);
    }

    public async UniTask<ItemContainer> FindItem(Pawn pawn, Vector2Int workPos, List<ItemDataContainer> require)
    {
        if (require == null)
        {
            return default;
        }
        for (int i = 0; i < require.Count; i++)
        {
            ItemDataContainer r = require[i];
            // skip if pawn already has enough
            if (pawn.GetItemCount(r.itemData, r.itemClass) >= r.amount)
                continue;

            Item item = World.Instance.FindNearestItem(
                    r.itemData,
                    r.itemClass,
                    workPos);

            if (item != null)
            {
                return new ItemContainer { item = item, amount = r.amount };
            }

            await UniTask.Yield();
        }
        return default;
    }
}


[Serializable]
public struct ItemDataContainer
{
    public ItemData itemData;
    public ItemClass itemClass;
    public int amount;
}

public struct ItemContainer
{
    public Item item;
    public int amount;
}