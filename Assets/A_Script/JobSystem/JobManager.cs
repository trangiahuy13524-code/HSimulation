using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class JobManager : MonoBehaviour
{
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

    private HashSet<WorldObject> usedJobObjects = new();

    public async UniTask<Job> GetJob(
    Pawn pawn,
    CancellationToken token)
    {
        Job bestJob = null;
        WorkPosition bestWorkPos = null;

        //float bestScore = float.MinValue;

        usedJobObjects.Clear();

        for (int i = availableJobs.Count - 1; i >= 0; i--)
        {
            if ((i & 7) == 0)
                await UniTask.Yield(token);

            Job job = availableJobs[i];

            if (job == null || job.removed)
            {
                availableJobs.RemoveAt(i);
                continue;
            }

            if (job.reserved || !job.available)
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

                foundPos =
                    AStarPathfinder.FindReachableWorkPosition(
                        pawn.CurrentGridPosition,
                        positions,
                        50);

                if (foundPos == null)
                    continue;

                bestWorkPos = foundPos;
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

        return bestJob;
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
        availableJobs.Remove(job);
    }

}