using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;



public class JobManager : MonoBehaviour
{
    [SerializeField] World world;
    public static JobManager Instance { get; private set; }

    void Start()
    {
        Instance = this;
        availableJobs.Clear();
    }

    List<Job> availableJobs = new();

    public void AddJob(Job job)
    {
        availableJobs.Add(job);
    }

    static SemaphoreSlim jobLock = new SemaphoreSlim(1, 1);
    public async UniTask<Job> GetJob(
    Pawn pawn,
    CancellationToken token)
    {
        await jobLock.WaitAsync(token);
        Job bestJob = null;
        WorkPosition bestWorkPos = null;

        //float bestScore = float.MinValue;

        HashSet<WorldObject> usedJobObjects = new();

        for (int i = availableJobs.Count - 1; i >= 0; i--)
        {
            await UniTask.Yield(token);

            Job job = availableJobs[i];

            if (job == null || job.removed)
            {
                availableJobs.RemoveAt(i);
                continue;
            }

            if (job.reserved)
                continue;

            if (!usedJobObjects.Add(job.refObject))
                continue;

            if (!pawn.QualifyForSkills(job.requiredSkills))
                continue;

            WorkPosition foundPos = null;

            if (job is WorkableJob workableJob)
            {
                WorkPosition[] positions =
                    workableJob.workBuilding.GetAvailableWorkPos();

                WorldThreadSafe worldTS = GetWTS();

                foundPos = await UniTask.RunOnThreadPool(() => AStarPathfinder.FindReachableWorkPosition(
                        pawn.CurrentGridPosition,
                        positions,
                        50,
                        worldTS));

                if (foundPos == null)
                    continue;

                bestWorkPos = foundPos;

                if (workableJob is CraftJob craftJob)
                {
                    craftJob.itemFound = await craftJob.craftBuilding.FindItem(pawn, bestWorkPos.workPos, craftJob.requiredItemDatas);
                    if (craftJob.itemFound.item == null)
                        continue;
                    await UniTask.Yield(token);
                }
            }

            bestJob = job;
            
            break;

            //float score = EvaluateJob(pawn, job);

            //if (score > bestScore)
            //{
            //    bestScore = score;
            //    bestJob = job;
            //    bestWorkPos = foundPos;
            //}
        }

        // reserve ONLY AFTER selection
        if (bestJob != null)
        {
            bestJob.reserved = true;

            if (bestJob is WorkableJob workableJob)
            {
                workableJob.reservedWorkPos = bestWorkPos;

                if (bestWorkPos != null)
                    bestWorkPos.occupied = true;
            }
        }

        jobLock.Release();
        return bestJob;
    }

    WorldThreadSafe GetWTS()
    {
        return new WorldThreadSafe(world.WorldSize, (byte[,])world.PawnCountOnGrid.Clone(), world.MaxPawnCount, (bool[,])world.NotPassableTiles.Clone());
    }
    float EvaluateJob(Pawn pawn, Job job)
    {
        Vector2Int diff =
            pawn.CurrentGridPosition -
            job.refObject.GetMidGrid();

        return -diff.sqrMagnitude;
    }

    public void ReturnJob(Job job)
    {
        job.reserved = false;
        //job.worker = null;
    }
    public void RemoveJob(Job job)
    {
        job.externalRemoved = true;
    }

}