using UnityEngine;

[CreateAssetMenu(menuName = "Game/Pawn/SpritePart")]
public class PartSprite : PartBase
{
    public ShapeBodyPart bodyPartShape;

    public Sprite eastSprite;
    public Sprite northSprite;
    public Sprite southSprite;
}
