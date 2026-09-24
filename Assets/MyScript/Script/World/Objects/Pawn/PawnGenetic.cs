using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Pawn
{
    [Header("Pawn Genetics")]
    [SerializeField] bool initialized;
    [SerializeField] GenomeRT genome;
    public GenomeRT Genome => genome;
    public override Sprite IconSprite => genome?.source?.raceIcon;

    public void InitializePawn(DataGenetics geneticData)
    {
        if (initialized) return;

        InitializeGenome(new GenomeRT(geneticData), geneticData.pawnSkills);
    }

    public void InitializePawn(GenomeRT mother, GenomeRT father)
    {
        if (initialized) return;

        InitializeGenome(new GenomeRT(mother, father), mother.source.pawnSkills);
    }

    private void InitializeGenome(GenomeRT runtimeGenome, IEnumerable<DataSkill> skills)
    {
        genome = runtimeGenome;
        SetBodySprite(genome.currentBody, genome.currentHead, genome.currentHair);
        pawnSkills = skills.ToDictionary(skill => skill, skill => (byte)0);
        initialized = true;
    }

    private void SetBodySprite(PartBioSprite bodySprite, PartBioSprite headSprite = null, PartBioSprite hairSprite = null)
    {
        bodyData.SetSpriteData(bodySprite);
        headData.SetSpriteData(headSprite);
        hairData.SetSpriteData(hairSprite);
    }
}
