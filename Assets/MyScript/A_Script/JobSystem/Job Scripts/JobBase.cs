using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class JobBase
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

    public abstract UniTask DoJob(Pawn worker, CancellationToken token);

    public abstract void ReturnJob(Pawn worker);

    protected virtual async UniTask FinishWork(Pawn worker, CancellationToken token)
    {
        await UniTask.Yield(token);
        result?.Invoke(worker);
    }
}