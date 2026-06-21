using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Research/ResearchNode")]
public class ResearchNode : ScriptableObject
{
    public ResearchData researchData;
    public List<ResearchNode> children;
}