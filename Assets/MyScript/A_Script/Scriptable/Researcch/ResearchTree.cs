using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Research/ResearchTree")]
public class ResearchTree : ScriptableObject
{
    public List<ResearchNode> mainNodes;
}