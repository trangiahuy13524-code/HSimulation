using System.Collections.Generic;
using UnityEngine;

public class ResearchManager : MonoBehaviour
{
    public static ResearchManager Instance;

    Dictionary<ResearchData, ResearchState> researches = new();

    void Start()
    {
        Instance = this;
    }

    public ResearchState GetState(ResearchData data)
    {
        if (!researches.TryGetValue(data, out var state))
        {
            state = new ResearchState(data);
            researches[data] = state;
        }

        return state;
    }

    public void AddProgress(
        ResearchData data,
        float amount)
    {
        var state = GetState(data);

        if (state.completed)
            return;

        state.progress += amount;

        if (state.progress >= data.totalProgress)
        {
            state.progress = data.totalProgress;
            state.completed = true;
        }
    }

    public bool IsCompleted(ResearchData data)
    {
        return GetState(data).completed;
    }
}