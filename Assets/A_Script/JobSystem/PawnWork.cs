using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Pawn : WorldObject
{
    public HashSet<Skill> pawnSkills = new();

    public bool HasAllSkills(IEnumerable<Skill> skills)
    {
        return skills.All(skill => pawnSkills.Contains(skill));
    }

    Job currentJob;
    
    public bool TryFindJob()
    {
        if (currentJob != null) return true;

        Job job = JobManager.Instance.GetJob(this);

        if (job != null)
        {
            currentJob = job;

            MakePath(job.workBuilding.CurrentGridPosition);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void RemoveJob()
    {
        currentJob = null;
    }
}