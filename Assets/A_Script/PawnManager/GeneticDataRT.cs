using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GeneticDataRT
{
    public string raceName;
    public string raceDescription;

    [Header("Base Stats")]
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

    public GeneticDataRT(GeneticData geneticData)
    {
        raceName = geneticData.raceName;
        raceDescription = geneticData.raceDescription;

        baseHealth = geneticData.baseHealth;
        baseMana = geneticData.baseMana;
        baseDefense = geneticData.baseDefense;
        baseAttack = geneticData.baseAttack;
        baseMagicAttack = geneticData.baseMagicAttack;
        baseSpeed = geneticData.baseSpeed;

        bodyData = new List<DirectionSpriteData>(geneticData.bodyData);
        headData = new List<DirectionSpriteData>(geneticData.headData);
        hairData = new List<DirectionSpriteData>(geneticData.hairData);
    }
}