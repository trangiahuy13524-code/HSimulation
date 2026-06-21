using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Research/ResearchData")]
public class ResearchData : ScriptableObject
{
    public string researchName;
    public string description;
    public Sprite icon;
    public int totalProgress;
    public List<SkillRequirement> requiredSkills;
}