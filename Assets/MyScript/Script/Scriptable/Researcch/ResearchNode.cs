using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Research/ResearchNode")]
public class ResearchNode : ScriptableObject
{
    public DataJobResearch researchData;
    public List<ResearchNode> children;
}