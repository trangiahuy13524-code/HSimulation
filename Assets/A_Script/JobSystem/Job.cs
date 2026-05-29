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

    public virtual bool ProgressCondition()
    {
        return true;
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

        reservedWorkPos.occupied = true;
        pawn.SetWorkPosition(reservedWorkPos);


        ActionResult result = await WorkRoutine(pawn, reservedWorkPos, token);

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
        pawn.ReturnJob();
    }
}

public class CraftJob : WorkableJob
{
    public List<RequireItemData> requiredItemDatas;
    public List<Item> reservedItems = new();
    public ItemOutputData outputItemData;
    public RequireItem itemFound;
    public BuildingCraft craftBuilding => (BuildingCraft)workBuilding;

    public void OnBuildingCraftDestroyed()
    {
        ClearReservedItems();
    }
    void AddReservedItem(Item item)
    {
        if (item != null)
        {
            item.reserved = true;
            reservedItems.Add(item);
        }
    }
    void ClearReservedItems()
    {
        foreach (var item in reservedItems)
        {
            if (item != null)
                item.reserved = false;
        }
        reservedItems.Clear();
    }
    void DestroyReservedItems()
    {
        foreach (var item in reservedItems)
        {
            if (item != null)
                item.Despawn();
        }
        reservedItems.Clear();
    }

    
    protected override async UniTask<ActionResult> WorkRoutine(
    Pawn pawn,
    WorkPosition wP,
    CancellationToken token)
    {
        ActionResult result = ActionResult.Success;
        // =========================================
        // COLLECT ALL REQUIRED ITEMS
        // =========================================
        bool first = true;
        for (int i = 0; i < requiredItemDatas.Count; i++)
        {
            RequireItemData required = requiredItemDatas[i];

            // how many still needed
            int remainingNeeded =
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

        // =========================================
        // DROP ALL REQUIRED ITEMS
        // =========================================

        ClearReservedItems();

        result = await pawn.MoveTo(wP.workPos, token);

        if (result != ActionResult.Success)
            return result;

        pawn.ChangeDirection(workBuilding.workDirection);
        await UniTask.Yield(token);

        for (int i = 0; i < requiredItemDatas.Count; i++)
        {
            RequireItemData required =
                requiredItemDatas[i];

            int amountInInventory =
                pawn.GetItemCount(
                    required.itemData,
                    required.itemClass);

            if (amountInInventory <= 0)
                continue;

            List<Item> droppedItems;
            (result, droppedItems) =
                await pawn.TryDrop(
                    required.itemData,
                    required.itemClass,
                    Mathf.Min(required.amount, amountInInventory),
                    token);

            foreach (var item in droppedItems)
            {
                AddReservedItem(item);
                Debug.Log(item + " " + item.StackCount);
            }

            if (result != ActionResult.Success)
                return result;

            await UniTask.Delay(100, cancellationToken: token);
        }

        // =========================================
        // DO WORK
        // =========================================

        return await pawn.DoProgressWork(
            workBuilding.workDirection,
            token);
    }

    protected override async UniTask FinishWork(Pawn pawn, CancellationToken token)
    {
        DestroyReservedItems();
        await UniTask.Yield(token);
        World.Instance.CreateItem(
            pawn.CurrentGridPosition + pawn.direction(),
            outputItemData.itemData,
            outputItemData.itemClass,
            outputItemData.amount);
        await base.FinishWork(pawn, token);
    }

    public override bool ProgressCondition()
    {
        return true;
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