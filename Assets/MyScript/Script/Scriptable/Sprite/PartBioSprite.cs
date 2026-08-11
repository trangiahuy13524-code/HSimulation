using UnityEngine;

[CreateAssetMenu(menuName = "Game/Pawn/BioSpritePart")]
public class PartBioSprite : PartSprite
{
    [Header("Biology")]
    public Vector2 attireOffset;
    public BodySex bodySex = BodySex.Both;
    public bool hybridable = false;
}