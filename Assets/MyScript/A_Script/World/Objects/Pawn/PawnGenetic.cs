using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Pawn
{
    
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
        SetBodySprite(genome.currentBody, genome.currentHead, genome.currentHair);
        

        //-----------------------------------
        // 3. Skills (CORRECT PLACE)
        //-----------------------------------
        pawnSkills = geneticData.pawnSkills
        .ToDictionary(skill => skill, skill => (byte)0);



        iconSprite = geneticData.raceIcon;
        initialized = true;
        }

    public void InitializePawn(GenomeRT mother, GenomeRT father)
    {
        if (initialized) return;

        //-----------------------------------
        // 1. Create Runtime Genome
        //-----------------------------------
        genome = new GenomeRT(mother, father);

        //-----------------------------------
        // 2. Generate Appearance
        //-----------------------------------
        SetBodySprite(genome.currentBody, genome.currentHead, genome.currentHair);

        //-----------------------------------
        // 3. Skills (CORRECT PLACE)
        //-----------------------------------
        pawnSkills = mother.source.pawnSkills
        .ToDictionary(skill => skill, skill => (byte)0);


        iconSprite = mother.source.raceIcon;
        initialized = true;
    }

    

    void SetBodySprite(SpritePart bodySprite, SpritePart headSprite = null, SpritePart hairSprite = null)
    {
        bodyData.SetDirectionSpriteData(bodySprite);
        headData.SetDirectionSpriteData(headSprite);
        hairData.SetDirectionSpriteData(hairSprite);
    }
}