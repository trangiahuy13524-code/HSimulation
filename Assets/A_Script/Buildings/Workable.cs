using UnityEngine;

public abstract class Workable : Building
{
    protected JobManager jobManager;
    [SerializeField] protected Vector2Int workPos;
    
    protected override void Start()
    {
        base.Start();
        jobManager = JobManager.Instance;
    }

    public abstract void PerformWork(Pawn pawn, Job job);
}