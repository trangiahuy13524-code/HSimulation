using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu]
public class DataGenetics : ScriptableObject
{
    public string raceName;
    public string raceDescription;
    public Sprite raceIcon;

    

    [Header("Reproduction")]
    public ReproductionType reproductionType;
    [Range(0, 1f)]
    public float femaleBirthChance = 0.5f;
    public BirthType birthType;

    [Header("Body")]
    public List<PartBodySprite> bodyData;
    public List<PartBioSprite> headData;
    public List<PartBioSprite> hairData;

    [Header("Skill")]
    public List<Skill> pawnSkills;

    [Header("Abilities")]
    public List<Ability> pawnAbilities;
}