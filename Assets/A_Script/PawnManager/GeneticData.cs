using UnityEngine;
using System.Collections.Generic;


public class GeneticData : ScriptableObject
{
    public string raceName;
    public string raceDescription;

    [Header("Base Stats")]
    public Sprite iconSprite;
    public int baseHealth;
    public int baseMana;
    public int baseDefense;
    public int baseAttack;
    public int baseMagicAttack;
    public int baseSpeed;

    [Header("Body")]
    public List<DirectionSpriteData> bodyData;
    public List<DirectionSpriteData> headData;
    public List<DirectionSpriteData> hairData;
}
