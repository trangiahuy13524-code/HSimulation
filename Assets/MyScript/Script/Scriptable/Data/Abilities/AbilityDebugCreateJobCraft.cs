using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Game/AbilityUI/DebugCreateJobCraft")]
public class AbilityDebugCreateJobCraft : DataAbility
{
    public byte jobIndex = 0;
    public override void Execute(WorldObject caster, Image image = null)
    {
        BuildingCraft building = caster as BuildingCraft;
        if (building != null)
        {
            if (jobIndex < 0 || jobIndex >= building.jobs.Count)
            {
                Debug.LogError($"Invalid jobIndex {jobIndex} for building {building.ThingName}");
                return;
            }
            building.CreateJob(building.jobs[jobIndex]);
        }
        else
        {
            Debug.LogError($"Caster {caster.ThingName} is not a BuildingCraft");
        }
    }
}
