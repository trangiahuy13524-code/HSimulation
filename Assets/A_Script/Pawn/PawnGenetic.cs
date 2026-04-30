using System.Collections.Generic;
using UnityEngine;

public partial class Pawn : WorldObject
{
    static readonly List<DirectionSpriteScriptable> spriteBuffer = new();
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

        initialized = true;
    }

    

    void GenerateBody(GeneticData data)
    {
        bool restrictBody = data.reproductionType == ReproductionType.Sexual;

        // Body => restricted only for sexual species
        DirectionSpriteScriptable body =
            PickValidSprite(data.bodyData, restrictBody);

        // Head => same rule as body
        DirectionSpriteScriptable head =
            PickValidSprite(data.headData, restrictBody);

        // Hair => same restricted
        DirectionSpriteScriptable hair =
            PickValidSprite(data.hairData, restrictBody);

        SetBodySprite(body, head, hair);
    }

    DirectionSpriteScriptable PickValidSprite(
    List<DirectionSpriteScriptable> list,
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

    void SetBodySprite(DirectionSpriteScriptable bodySprite, DirectionSpriteScriptable headSprite = null, DirectionSpriteScriptable hairSprite = null)
    {
        bodyData.SetDirectionSpriteData(bodySprite);
        headData.SetDirectionSpriteData(headSprite);
        hairData.SetDirectionSpriteData(hairSprite);
    }
}