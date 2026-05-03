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

    [SerializeField] Queue<Job> availableJobs = new();


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

            if (pawn.HasAllSkills(job.requiredSkills))
                return job;

            availableJobs.Enqueue(job);
        }

        return null;
    }
}
