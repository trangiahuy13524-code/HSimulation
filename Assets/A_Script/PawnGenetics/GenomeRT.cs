using System;
using UnityEngine;

[Serializable]
public class GenomeRT
{
    public GeneticData source;

    public int health;
    public int mana;
    public int defense;
    public int attack;
    public float magicPotential;
    public float speed;
    public Sex sex;

    public BodySpritePart currentBody;
    public BodySpritePart currentHead;
    public BodySpritePart currentHair;
    public int generation;

    public GenomeRT(GeneticData geneticData)
    {
        source = geneticData;
        //generation = 0;

        //-----------------------------------
        // 2. Determine Sex
        //-----------------------------------
        switch (geneticData.reproductionType)
        {
            case ReproductionType.None:
                sex = Sex.None;
                break;

            case ReproductionType.Sexual:
                sex =
                    UnityEngine.Random.value <= geneticData.femaleBirthChance
                    ? Sex.Female
                    : Sex.Male;
                break;

            case ReproductionType.Hermaphroditic:
                sex = Sex.Hermaphrodite;
                break;

            case ReproductionType.Asexual:
                sex = Sex.Asexual;
                break;
        }

        //-----------------------------------
        // 3. Apply Genetic Variance
        //-----------------------------------
        health = RandomizeStat(geneticData.baseHealth);
        mana = RandomizeStat(geneticData.baseMana);
        defense = RandomizeStat(geneticData.baseDefense);
        attack = RandomizeStat(geneticData.baseAttack);
        magicPotential = RandomizeStat(geneticData.magicPotential);
        speed = RandomizeStat(geneticData.baseSpeed);
    }

    public GenomeRT(GenomeRT parent)
    {
        source = parent.source;
        generation = parent.generation + 1;
        float mul = 1f + generation / 10;
        //-----------------------------------
        // INT STATS
        //-----------------------------------
        health = ApplyGeneration(source.baseHealth, mul);
        mana = ApplyGeneration(source.baseMana, mul);
        defense = ApplyGeneration(source.baseDefense, mul);
        attack = ApplyGeneration(source.baseAttack, mul);
        magicPotential = ApplyGeneration(source.magicPotential, mul);

        //-----------------------------------
        // FLOAT STATS
        //-----------------------------------
        speed = ApplyGeneration(source.baseSpeed, mul);

        //-----------------------------------
        // Sex
        //-----------------------------------
        sex =
            UnityEngine.Random.value <= source.femaleBirthChance
            ? Sex.Female
            : Sex.Male;
    }

    static int offset;
    int RandomizeStat(int baseValue)
    {
        offset = baseValue / 3;
        return baseValue + UnityEngine.Random.Range(-offset, offset + 1);
    }

    static float offsetF;
    float RandomizeStat(float baseValue)
    {
        offsetF = baseValue / 3f;
        return baseValue + UnityEngine.Random.Range(-offsetF, offsetF);
    }

    int ApplyGeneration(int baseValue, float mul)
    {
        return RandomizeStat((int)(baseValue * mul));
    }

    float ApplyGeneration(float baseValue, float mul)
    {
        return RandomizeStat(baseValue * mul);
    }
}