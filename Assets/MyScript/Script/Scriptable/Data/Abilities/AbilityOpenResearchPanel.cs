using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Game/AbilityUI/OpenResearchPanel")]
public class AbilityOpenResearchPanel : DataAbility
{

    public override void Execute(WorldObject caster, Image image = null)
    {
        if (caster is BuildingResearch buildingResearch)
        {
            ResearchPanel researchPanel = ResearchPanel.Instance;

            if (researchPanel != null)
            {
                researchPanel.researchPanel.gameObject.SetActive(true);

                researchPanel.CreateResearchUI(buildingResearch);
                
            }
            else
            {
                Debug.LogError("ResearchPanel instance not found.");
            }
        }
    }

    
}