using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class Work
{
    public short finishedWorks = 0;
    public List<Action> works;

    public Work(List<Action> performAction)
    {
        works = new List<Action>(performAction);

        for (int i = 0; i < works.Count; i++)
        {
            works[i] += () =>
            {
                finishedWorks++;
            };
        }
    }
}
