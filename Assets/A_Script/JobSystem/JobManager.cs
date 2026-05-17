using System.Collections.Generic;
using UnityEngine;

public class JobManager : MonoBehaviour
{
    public static JobManager Instance { get; private set; }

    void Start()
    {
        Instance = this;
        availableJobs.Clear();
    }

    [SerializeField] List<Job> availableJobs = new();

    public void AddJob(Job job)
    {
        availableJobs.Add(job);
    }

    private HashSet<Workable> usedWorkables = new();

    public Job GetJob(Pawn pawn)
    {
        Job bestJob = null;
        float bestScore = float.MinValue;

        usedWorkables.Clear();

        // CLEANUP + SEARCH in ONE PASS
        for (int i = availableJobs.Count - 1; i >= 0; i--)
        {
            Job job = availableJobs[i];

            // remove invalid immediately
            if (job == null ||
                job.workBuilding == null)
            {
                availableJobs.RemoveAt(i);
                continue;
            }

            if (job.reserved)
                continue;

            if (!usedWorkables.Add(job.workBuilding))
                continue;

            if (!pawn.QualifyForSkills(job.requiredSkills))
                continue;

            float score = EvaluateJob(pawn, job);

            if (score > bestScore)
            {
                bestScore = score;
                bestJob = job;
            }
        }

        if (bestJob != null)
            bestJob.reserved = true;

        return bestJob;
    }
    float EvaluateJob(Pawn pawn, Job job)
    {
        Vector2Int diff =
            pawn.CurrentGridPosition -
            job.workBuilding.GetMidGrid();

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

    //private void Update()
    //{
    //    if (unreachableJobs.Count > 0)
    //    {
    //        if (currentTime > refreshUnreachableJobDuration)
    //        {
    //            currentTime = 0;
    //            AddUnreachableJobToAvailableJob();
    //        }
    //        else
    //        {
    //            currentTime += Time.deltaTime;
    //        }
    //    }
    //    else
    //    {
    //        currentTime = 0;
    //    }
    //}

}
