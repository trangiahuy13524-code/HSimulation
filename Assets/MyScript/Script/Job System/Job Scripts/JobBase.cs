using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

public abstract class JobBase
{
    public abstract WorldObject refObject { get; }
    public IEnumerable<SkillRequirement> requiredSkills;
    public float currentProgress = 0;
    public float totalProgress = 0;
    public virtual bool removed => externalRemoved;
    public bool externalRemoved = false;
    public bool reserved;
    public Action<Pawn> result;

    public abstract UniTask DoJob(Pawn worker, CancellationToken token);
    public abstract void ReturnJob(Pawn worker);

    public virtual void OnJobCancelled(Pawn worker)
    {
        worker.ReturnJob();
    }

    protected async UniTask ExecuteWork(
        Pawn worker,
        CancellationToken token,
        Func<UniTask> workRoutine)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            await workRoutine();
            token.ThrowIfCancellationRequested();
            await FinishWork(worker, token);
            worker.RemoveJob();
        }
        catch (OperationCanceledException)
        {
            OnJobCancelled(worker);
            throw;
        }
        catch
        {
            worker.ReturnJob();
            throw;
        }
    }

    protected virtual async UniTask FinishWork(Pawn worker, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        result?.Invoke(worker);
        await UniTask.CompletedTask;
    }
}
