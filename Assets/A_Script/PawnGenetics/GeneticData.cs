using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu]
public class GeneticData : ScriptableObject
{
    public string raceName;
    public string raceDescription;
    public Sprite raceIcon;

    [Header("Base Stats")]
    public int baseHealth;
    public int baseMana;
    public int baseDefense;
    public int baseAttack;
    public float magicPotential;
    public float baseSpeed;

    [Header("Reproduction")]
    public ReproductionType reproductionType;
    [Range(0, 1f)]
    public float femaleBirthChance = 0.5f;
    public BirthType birthType;

    [Header("Body")]
    public List<BodySpritePart> bodyData;
    public List<BodySpritePart> headData;
    public List<BodySpritePart> hairData;

    [Header("Skill")]
    public List<Skill> pawnSkills;
}