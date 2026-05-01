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
    public int magicAttack;
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
        health = RandomizeStatInt(geneticData.baseHealth);
        mana = RandomizeStatInt(geneticData.baseMana);
        defense = RandomizeStatInt(geneticData.baseDefense);
        attack = RandomizeStatInt(geneticData.baseAttack);
        magicAttack = RandomizeStatInt(geneticData.baseMagicAttack);
        speed = RandomizeStatFloat(geneticData.baseSpeed);
    }

    public GenomeRT(GenomeRT parent)
    {
        source = parent.source;
        generation = parent.generation + 1;
        float mul = 1f + generation / 20;
        //-----------------------------------
        // INT STATS
        //-----------------------------------
        health = ApplyGenerationInt(source.baseHealth, mul);
        mana = ApplyGenerationInt(source.baseMana, mul);
        defense = ApplyGenerationInt(source.baseDefense, mul);
        attack = ApplyGenerationInt(source.baseAttack, mul);
        magicAttack = ApplyGenerationInt(source.baseMagicAttack, mul);

        //-----------------------------------
        // FLOAT STATS
        //-----------------------------------
        speed = ApplyGenerationFloat(source.baseSpeed, mul);

        //-----------------------------------
        // Sex
        //-----------------------------------
        sex =
            UnityEngine.Random.value <= source.femaleBirthChance
            ? Sex.Female
            : Sex.Male;
    }

    static int offset;
    int RandomizeStatInt(int baseValue)
    {
        offset = baseValue / 3;
        return baseValue + UnityEngine.Random.Range(-offset, offset + 1);
    }

    static float offsetF;
    float RandomizeStatFloat(float baseValue)
    {
        offsetF = baseValue / 3f;
        return baseValue + UnityEngine.Random.Range(-offsetF, offsetF);
    }

    int ApplyGenerationInt(int baseValue, float mul)
    {
        return RandomizeStatInt((int)(baseValue * mul));
    }

    float ApplyGenerationFloat(float baseValue, float mul)
    {
        return RandomizeStatFloat(baseValue * mul);
    }
}