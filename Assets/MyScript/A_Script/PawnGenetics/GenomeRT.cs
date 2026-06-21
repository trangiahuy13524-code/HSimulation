using System;
using System.Collections.Generic;
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
    public SpritePart currentHead;
    public SpritePart currentHair;
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
        // 3. Apply Body
        //-----------------------------------
        GenerateBody(geneticData);

        //-----------------------------------
        // 4. Apply Genetic Variance
        //-----------------------------------
        health = RandomizeStat(currentBody.baseHealth);
        mana = RandomizeStat(currentBody.baseMana);
        defense = RandomizeStat(currentBody.baseDefense);
        attack = RandomizeStat(currentBody.baseAttack);
        magicPotential = RandomizeStat(currentBody.magicPotential);
        speed = RandomizeStat(currentBody.baseSpeed);
    }

    public GenomeRT(GenomeRT mother, GenomeRT father)
    {
        source = mother.source;

        //-----------------------------------
        // Apply Body
        //-----------------------------------
        GenerateBody(mother, father);


        //-----------------------------------
        // INT STATS
        //-----------------------------------
        generation = mother.generation + 1;
        float mul = 1f + generation / 10;
        health = ApplyGeneration(currentBody.baseHealth, mul);
        mana = ApplyGeneration(currentBody.baseMana, mul);
        defense = ApplyGeneration(currentBody.baseDefense, mul);
        attack = ApplyGeneration(currentBody.baseAttack, mul);
        magicPotential = ApplyGeneration(currentBody.magicPotential, mul);

        //-----------------------------------
        // FLOAT STATS
        //-----------------------------------
        speed = ApplyGeneration(currentBody.baseSpeed, mul);

        //-----------------------------------
        // Sex
        //-----------------------------------
        sex =
            UnityEngine.Random.value <= source.femaleBirthChance
            ? Sex.Female
            : Sex.Male;
    }

    static readonly List<SpritePart> spriteBuffer = new();
    static readonly List<BodySpritePart> bodySpriteBuffer = new();
    void GenerateBody(GeneticData data)
    {
        bool restrictBody = data.reproductionType == ReproductionType.Sexual;

        // Body => restricted only for sexual species
        BodySpritePart body = PickValidSprite(data.bodyData, restrictBody);

        // Head => same rule as body
        SpritePart head = PickValidSprite(data.headData, restrictBody);

        // Hair => same restricted
        SpritePart hair = PickValidSprite(data.hairData, restrictBody);

        currentBody = body;
        currentHead = head;
        currentHair = hair;
    }

    void GenerateBody(GenomeRT mother, GenomeRT father)
    {
        bool restrictBody =
            mother.source.reproductionType == ReproductionType.Sexual;

        currentBody = InheritPart(
            mother.currentBody,
            father.currentBody,
            mother.source.bodyData,
            restrictBody);

        currentHead = InheritPart(
            mother.currentHead,
            father.currentHead,
            mother.source.headData,
            restrictBody);

        currentHair = InheritPart(
            mother.currentHair,
            father.currentHair,
            mother.source.hairData,
            restrictBody);
    }
    SpritePart InheritPart(
    SpritePart motherPart,
    SpritePart fatherPart,
    List<SpritePart> speciesPool,
    bool restrict)
    {
        spriteBuffer.Clear();
        spriteBuffer.Add(motherPart);
        if (fatherPart != null)
        {
            spriteBuffer.Add(fatherPart);
        }

        bool inheritFromParent = UnityEngine.Random.Range(0, 10) > 6;

        return inheritFromParent
            ? PickValidSprite(spriteBuffer, restrict)
            : PickValidSprite(speciesPool, restrict);
    }

    BodySpritePart InheritPart(
    BodySpritePart motherPart,
    BodySpritePart fatherPart,
    List<BodySpritePart> speciesPool,
    bool restrict)
    {
        bodySpriteBuffer.Clear();
        bodySpriteBuffer.Add(motherPart);
        if (fatherPart != null)
        {
            bodySpriteBuffer.Add(fatherPart);
        }

        bool inheritFromParent = UnityEngine.Random.Range(0, 10) > 6;

        return inheritFromParent
            ? PickValidSprite(bodySpriteBuffer, restrict)
            : PickValidSprite(speciesPool, restrict);
    }

    SpritePart PickValidSprite(
    List<SpritePart> list,
    bool restrictBySex)
    {
        if (list == null || list.Count == 0)
            return null;

        // No restriction => fast random pick
        if (!restrictBySex)
            return list[UnityEngine.Random.Range(0, list.Count)];

        spriteBuffer.Clear();

        foreach (var sprite in list)
        {
            if (sprite.bodySex == BodySex.Both)
            {
                spriteBuffer.Add(sprite);
            }
            // STRICT sexual reproduction rule
            if (sex == Sex.Male &&
                sprite.bodySex == BodySex.Male)
            {
                spriteBuffer.Add(sprite);
            }

            if (sex == Sex.Female &&
                sprite.bodySex == BodySex.Female)
            {
                spriteBuffer.Add(sprite);
            }
        }

        // Safety fallback
        if (spriteBuffer.Count == 0)
            return list[UnityEngine.Random.Range(0, list.Count)];

        return spriteBuffer[UnityEngine.Random.Range(0, spriteBuffer.Count)];
    }

    BodySpritePart PickValidSprite(
    List<BodySpritePart> list,
    bool restrictBySex)
    {
        if (list == null || list.Count == 0)
            return null;

        // No restriction => fast random pick
        if (!restrictBySex)
            return list[UnityEngine.Random.Range(0, list.Count)];

        bodySpriteBuffer.Clear();

        foreach (var sprite in list)
        {
            if (sprite.bodySex == BodySex.Both)
            {
                bodySpriteBuffer.Add(sprite);
            }
            // STRICT sexual reproduction rule
            if (sex == Sex.Male &&
                sprite.bodySex == BodySex.Male)
            {
                bodySpriteBuffer.Add(sprite);
            }

            if (sex == Sex.Female &&
                sprite.bodySex == BodySex.Female)
            {
                bodySpriteBuffer.Add(sprite);
            }
        }

        // Safety fallback
        if (bodySpriteBuffer.Count == 0)
            return list[UnityEngine.Random.Range(0, list.Count)];

        return bodySpriteBuffer[UnityEngine.Random.Range(0, bodySpriteBuffer.Count)];
    }

    int RandomizeStat(int baseValue)
    {
        int offset = baseValue / 3;
        return baseValue + UnityEngine.Random.Range(-offset, offset + 1);
    }

    float RandomizeStat(float baseValue)
    {
        float offsetF = baseValue / 3f;
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