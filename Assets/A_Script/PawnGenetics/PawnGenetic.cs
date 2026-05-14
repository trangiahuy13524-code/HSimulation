using System.Collections.Generic;
using System.Linq;
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
        pawnSkills = geneticData.pawnSkills
        .ToDictionary(skill => skill, skill => (byte)0);

            iconSprite = geneticData.raceIcon;

            initialized = true;
        }

    public void InitializePawn(GenomeRT mother)
    {
        if (initialized) return;

        //-----------------------------------
        // 1. Create Runtime Genome
        //-----------------------------------
        genome = new GenomeRT(mother);

        //-----------------------------------
        // 2. Generate Appearance
        //-----------------------------------
        GenerateBody(mother);

        //-----------------------------------
        // 3. Skills (CORRECT PLACE)
        //-----------------------------------
        pawnSkills = mother.source.pawnSkills
        .ToDictionary(skill => skill, skill => (byte)0);


        iconSprite = mother.source.raceIcon;

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

    void GenerateBody(GenomeRT mother)
    {
        bool restrictBody = mother.source.reproductionType == ReproductionType.Sexual;

        // Body => restricted only for sexual species
        BodySpritePart body = Random.Range(0, 10) > 6 ?
            mother.currentBody : PickValidSprite(mother.source.bodyData, restrictBody);

        // Head => same rule as body
        BodySpritePart head = Random.Range(0, 10) > 6 ?
            mother.currentHead : PickValidSprite(mother.source.headData, restrictBody);

        // Hair => same restricted
        BodySpritePart hair = Random.Range(0, 10) > 6 ?
            mother.currentHair : PickValidSprite(mother.source.hairData, restrictBody);

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