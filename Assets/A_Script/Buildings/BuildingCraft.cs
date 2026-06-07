using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingCraft : BuildingWorkable
{
    [SerializeField] List<RequireItemData> requiredItems;
    [SerializeField] ItemOutputData ItemOutputData;

    // =====================================================
    // FIND NEAREST REQUIRED ITEM
    // =====================================================

    static SemaphoreSlim findItemLock = new SemaphoreSlim(1, 1);
    public async UniTask<RequireItem> FindItem(Pawn pawn, Vector2Int workPos, List<RequireItemData> require)
    {
        await findItemLock.WaitAsync();
        if (require == null)
        {
            findItemLock.Release();
            return default;
        }
        for (int i = 0; i < require.Count; i++)
        {
            RequireItemData r = require[i];
            // skip if pawn already has enough
            if (pawn.GetItemCount(r.itemData, r.itemClass) >= r.amount)
                continue;

            Item item =
                world.FindNearestItem(
                    r.itemData,
                    r.itemClass,
                    workPos);

            if (item != null)
            {
                findItemLock.Release();
                return new RequireItem { item = item, amount = r.amount };
            }

            await UniTask.Yield();
        }
        findItemLock.Release();
        return default;
    }

    // =====================================================
    // JOB
    // =====================================================

    public void CreateJob()
    {
        CraftJob job = new();

        job.requiredItemDatas = requiredItems;
        job.outputItemData = ItemOutputData;
        job.requiredSkills = requiredSkills;
        job.workBuilding = this;

        job.totalProgress = 100;

        job.result = (pawn) =>
        {
            Debug.Log("Craft Finished");
        };

        //onDestroy += job.OnBuildingCraftDestroyed;
        jobManager.AddJob(job);
    }


    private void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            CreateJob();
        }
    }

    
}