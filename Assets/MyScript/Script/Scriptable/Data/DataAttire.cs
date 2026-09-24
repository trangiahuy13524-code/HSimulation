using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item/Attire/AttireData")]
public class DataAttire : DataItem
{
    [Header("Attire")]
    public BodyTag bodyTag = BodyTag.OffBody;
    public int wearingTime = 10;
    public PartAttire[] attireParts;

    public override bool IsStackable => false;

    public bool HasAttirePart(ShapeBodyPart bodyShape)
    {
        return GetAttirePart(bodyShape) != null;
    }

    public PartAttire GetAttirePart(ShapeBodyPart bodyShape)
    {
        if (attireParts == null || attireParts.Length == 0)
            return null;

        foreach (var part in attireParts)
        {
            if (part?.bodyPartShape == bodyShape)
            {
                return part;
            }
        }

        return null;
    }
}