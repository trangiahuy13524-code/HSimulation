using System.Collections.Generic;
using UnityEngine;

public class JobManager : MonoBehaviour
{
    public static JobManager Instance { get; private set; }
    public float refreshUnreachableJobDuration = 1f;
    [SerializeField] float currentTime = 0;
    void Start()
    {
        Instance = this;
        availableJobs.Clear();
    }

    Queue<Job> availableJobs = new();
    Queue<Job> unreachableJobs = new();

    public void AddJob(Job job)
    {
        availableJobs.Enqueue(job);
    }

    public Job GetJob(Pawn pawn)
    {
        int count = availableJobs.Count;

        for (int i = 0; i < count; i++)
        {
            Job job = availableJobs.Dequeue();
            if (job.removed || job.workBuilding == null) continue;

            if (pawn.QualifyForSkills(job.requiredSkills))
            {
                return job;
            }

            availableJobs.Enqueue(job);
        }

        return null;
    }

    public void ReturnJob(Job returnedJob)
    {
        unreachableJobs.Enqueue(returnedJob);
    }

    private void Update()
    {
        if (unreachableJobs.Count > 0)
        {
            if (currentTime > refreshUnreachableJobDuration)
            {
                currentTime = 0;
                AddUnreachableJobToAvailableJob();
            }
            else
            {
                currentTime += Time.deltaTime;
            }
        }
        else
        {
            currentTime = 0;
        }
    }
    void AddUnreachableJobToAvailableJob()
    {
        Job job = unreachableJobs.Dequeue();
        if (job != null)
        {
            if (job.removed || job.workBuilding == null)
            {
                return;
            }
        }
        else
        {
            return;
        }
        availableJobs.Enqueue(job);
    }
}
