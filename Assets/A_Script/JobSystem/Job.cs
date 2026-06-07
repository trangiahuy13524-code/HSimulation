using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class Job
{
    public abstract WorldObject refObject { get; }
    public IEnumerable<SkillRequirement> requiredSkills;
    public float currentProgress = 0;
    public float totalProgress = 0;
    public virtual bool removed => externalRemoved;
    public bool externalRemoved = false;
    public bool reserved;
    //public Pawn worker;
    public Action<Pawn> result;

    public abstract UniTask DoJob(
    Pawn worker,
    CancellationToken token);

    public virtual void ReturnJob()
    {
        
    }
}

public class WorkableJob : Job
{
    public override WorldObject refObject => workBuilding;
    public BuildingWorkable workBuilding;
    public WorkPosition reservedWorkPos;
    public override bool removed => workBuilding == null || externalRemoved;
    public override UniTask DoJob(
    Pawn worker,
    CancellationToken token)
    {
        return WorkToDo(worker, token);
    }

    async UniTask WorkToDo(
    Pawn pawn,
    CancellationToken token)
    {

        if (reservedWorkPos == null)
        {
            pawn.ReturnJob();
            return;
        }


        ActionResult result = await WorkRoutine(pawn, reservedWorkPos, token);
        Debug.Log($"Work Routine Result: {result}");

        switch (result)
        {
            case ActionResult.Success:
                await FinishWork(pawn, token);
                pawn.RemoveJob();
                break;
            case ActionResult.Cancelled:
                OnJobCancelled(pawn);
                break;
        }
    }

    protected virtual async UniTask FinishWork(Pawn pawn, CancellationToken token)
    {
        await UniTask.Yield(token);
        result?.Invoke(pawn);
    }

    protected virtual async UniTask<ActionResult> WorkRoutine(
    Pawn pawn,
    WorkPosition wP,
    CancellationToken token)
    {
        var moveResult = await pawn.MoveTo(wP.workPos, token);

        if (moveResult != ActionResult.Success)
            return moveResult;

        return await pawn.DoProgressWork(
            workBuilding.workDirection,
            token);
    }

    public virtual void OnJobCancelled(Pawn pawn)
    {
        Debug.Log("Job Cancelled");
        pawn.ReturnJob();
    }

    public override void ReturnJob()
    {
        if (reservedWorkPos != null)
        {
            reservedWorkPos.occupied = false;
            reservedWorkPos = null;
        }
    }
}

public class CraftJob : WorkableJob
{
    public List<RequireItemData> requiredItemDatas;
    public ItemData onCraftItemData;
    private Item onCraftItem;
    public ItemOutputData outputItemData;
    public RequireItem itemFound;
    public BuildingCraft craftBuilding => (BuildingCraft)workBuilding;

    
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
            for (int i = 0; i < requiredItemDatas.Count; i++)
            {
                RequireItemData required = requiredItemDatas[i];

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
                        itemFound = await craftBuilding.FindItem(pawn, wP.workPos, requiredItemDatas);
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
            hasOnCraftItem = true;
            (result, remainingNeeded) = await pawn.MoveToAndPickUp(
                            onCraftItem,
                            1,
                            token);

            if (result != ActionResult.Success)
            {
                return result;
            }
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
            pawn.RemoveItems(requiredItemDatas);
            onCraftItem = World.Instance.CreateItem(
                pawn.CurrentGridPosition + pawn.direction(),
                onCraftItemData,
                outputItemData.itemClass,
                1)[0];
            onCraftItem.reserved = true;
        }
        else
        {
            
        }

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

    public override void OnJobCancelled(Pawn pawn)
    {
        onCraftItem.reserved = false;
        base.OnJobCancelled(pawn);
    }

    protected override async UniTask FinishWork(Pawn pawn, CancellationToken token)
    {
        
        await UniTask.Yield(token);
        World.Instance.CreateItem(
            pawn.CurrentGridPosition + pawn.direction(),
            outputItemData.itemData,
            outputItemData.itemClass,
            outputItemData.amount);
        await base.FinishWork(pawn, token);
    }
}

[Serializable]
public struct RequireItemData
{
    public ItemData itemData;
    public ItemClass itemClass;
    public int amount;
}

public struct RequireItem
{
    public Item item;
    public int amount;
}