using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;



public class JobManager : MonoBehaviour
{
    [SerializeField] World world;
    [SerializeField] ResearchManager researchManager;
    public static JobManager Instance { get; private set; }

    void Start()
    {
        Instance = this;
        availableJobs.Clear();
    }

    List<JobBase> availableJobs = new();

    public void AddJob(JobBase job)
    {
        availableJobs.Add(job);
    }

    SemaphoreSlim jobLock = new SemaphoreSlim(1, 1);
    HashSet<WorldObject> usedJobObjects = new();
    public async UniTask<JobBase> GetJob(
    Pawn pawn,
    CancellationToken token)
    {
        await jobLock.WaitAsync(token);
        JobBase bestJob = null;
        WorkPosition bestWorkPos = null;

        //float bestScore = float.MinValue;
        usedJobObjects.Clear();
        for (int i = availableJobs.Count - 1; i >= 0; i--)
        {
            await UniTask.Yield(token);

            JobBase job = availableJobs[i];

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

            if (job is JobBuilding workableJob)
            {
                JobCraft craftJob = workableJob as JobCraft;
                if (craftJob != null)
                {
                    if (!craftJob.recipeData.IsUnlocked())
                    {
                        continue;
                    }
                }

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

                if (craftJob != null)
                {
                    craftJob.itemFound = await craftJob.FindItem(pawn, bestWorkPos.workPos, craftJob.recipeData.requiredItems);
                    if (craftJob.itemFound.item == null)
                        continue;
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

            if (bestJob is JobBuilding workableJob)
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
        return new WorldThreadSafe(world.WorldSize, (byte[,])world.pawnCountOnGrid.Clone(), world.MaxPawnCount, (bool[,])world.notPassableTiles.Clone());
    }
    //float EvaluateJob(Pawn pawn, Job job)
    //{
    //    Vector2Int diff =
    //        pawn.CurrentGridPosition -
    //        job.refObject.GetMidGrid();

    //    return -diff.sqrMagnitude;
    //}

    public void ReturnJob(JobBase job)
    {
        job.reserved = false;
        job.ReturnJob();
    }
    public void RemoveJob(JobBase job)
    {
        job.externalRemoved = true;
    }

}