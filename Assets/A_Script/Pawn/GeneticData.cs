using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu]
public class GeneticData : ScriptableObject
{
    public string raceName;
    public string raceDescription;

    [Header("Base Stats")]
    public int baseHealth;
    public int baseMana;
    public int baseDefense;
    public int baseAttack;
    public int baseMagicAttack;
    public float baseSpeed;
    public int offsetRange;

    [Header("Reproduction")]
    public ReproductionType reproductionType;
    [Range(0, 1f)]
    public float femaleBirthChance = 0.5f;
    public BirthType birthType;

    [Header("Body")]
    public List<DirectionSpriteScriptable> bodyData;
    public List<DirectionSpriteScriptable> headData;
    public List<DirectionSpriteScriptable> hairData;
}