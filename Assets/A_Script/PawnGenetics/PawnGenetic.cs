using System.Collections.Generic;
using UnityEngine;

public partial class Pawn : WorldObject
{
    static readonly List<BodySpritePart> spriteBuffer = new();
    [SerializeField] bool initialized = false;
    [SerializeField] GenomeRT genome;
    public GenomeRT Genome => genome;

    public void InitializePawn(GeneticData geneticData)
    {
        if (initialized) return;

        //-----------------------------------
        // 1. Create Runtime Genome
        //-----------------------------------
        genome = new GenomeRT(geneticData);

        //-----------------------------------
        // 2. Generate Appearance
        //-----------------------------------
        GenerateBody(geneticData);

        //-----------------------------------
        // 3. Skills (CORRECT PLACE)
        //-----------------------------------
        pawnSkills = new HashSet<Skill>(geneticData.pawnSkills);

        iconSprite = geneticData.raceIcon;

        initialized = true;
    }

    public void InitializePawn(GenomeRT parent)
    {
        if (initialized) return;

        //-----------------------------------
        // 1. Create Runtime Genome
        //-----------------------------------
        genome = new GenomeRT(parent);

        //-----------------------------------
        // 2. Generate Appearance
        //-----------------------------------
        GenerateBody(parent);

        //-----------------------------------
        // 3. Skills (CORRECT PLACE)
        //-----------------------------------
        pawnSkills = new HashSet<Skill>(parent.source.pawnSkills);

        iconSprite = parent.source.raceIcon;

        initialized = true;
    }

    void GenerateBody(GeneticData data)
    {
        bool restrictBody = data.reproductionType == ReproductionType.Sexual;

        // Body => restricted only for sexual species
        BodySpritePart body = PickValidSprite(data.bodyData, restrictBody);

        // Head => same rule as body
        BodySpritePart head = PickValidSprite(data.headData, restrictBody);

        // Hair => same restricted
        BodySpritePart hair = PickValidSprite(data.hairData, restrictBody);

        genome.currentBody = body;
        genome.currentHead = head;
        genome.currentHair = hair;
        SetBodySprite(body, head, hair);
    }

    void GenerateBody(GenomeRT parent)
    {
        bool restrictBody = parent.source.reproductionType == ReproductionType.Sexual;

        // Body => restricted only for sexual species
        BodySpritePart body = Random.Range(0, 10) > 6 ?
            parent.currentBody : PickValidSprite(parent.source.bodyData, restrictBody);

        // Head => same rule as body
        BodySpritePart head = Random.Range(0, 10) > 6 ?
            parent.currentHead : PickValidSprite(parent.source.headData, restrictBody);

        // Hair => same restricted
        BodySpritePart hair = Random.Range(0, 10) > 6 ?
            parent.currentHair : PickValidSprite(parent.source.hairData, restrictBody);

        genome.currentBody = body;
        genome.currentHead = head;
        genome.currentHair = hair;
        SetBodySprite(body, head, hair);
    }

    BodySpritePart PickValidSprite(
    List<BodySpritePart> list,
    bool restrictBySex)
    {
        if (list == null || list.Count == 0)
            return null;

        // No restriction => fast random pick
        if (!restrictBySex)
            return list[Random.Range(0, list.Count)];

        spriteBuffer.Clear();

        foreach (var sprite in list)
        {
            if (sprite.bodySex == BodySex.Both)
            {
                spriteBuffer.Add(sprite);
            }
            // STRICT sexual reproduction rule
            if (genome.sex == Sex.Male &&
                sprite.bodySex == BodySex.Male)
            {
                spriteBuffer.Add(sprite);
            }

            if (genome.sex == Sex.Female &&
                sprite.bodySex == BodySex.Female)
            {
                spriteBuffer.Add(sprite);
            }
        }

        // Safety fallback
        if (spriteBuffer.Count == 0)
            return list[Random.Range(0, list.Count)];

        return spriteBuffer[Random.Range(0, spriteBuffer.Count)];
    }

    void SetBodySprite(BodySpritePart bodySprite, BodySpritePart headSprite = null, BodySpritePart hairSprite = null)
    {
        bodyData.SetDirectionSpriteData(bodySprite);
        headData.SetDirectionSpriteData(headSprite);
        hairData.SetDirectionSpriteData(hairSprite);
    }
}