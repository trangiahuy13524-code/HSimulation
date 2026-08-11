using System.Collections.Generic;
using UnityEngine;

public class ResearchManager : MonoBehaviour
{
    public static ResearchManager Instance;

    Dictionary<DataJobResearch, ResearchState> researches = new();

    void Start()
    {
        Instance = this;
    }

    public ResearchState GetState(DataJobResearch data)
    {
        if (!researches.TryGetValue(data, out var state))
        {
            state = new ResearchState(data);
            researches[data] = state;
        }

        return state;
    }

    public void AddProgress(
        DataJobResearch data,
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

    public bool IsCompleted(DataJobResearch data)
    {
        return GetState(data).completed;
    }
}