using UnityEngine;

[CreateAssetMenu(menuName = "Game/PawnSprite/SpritePart")]
public class SpritePart : SpriteScriptableBase
{
    public BodyPartShape bodyPartShape;

    public Sprite eastSprite;
    public Sprite northSprite;
    public Sprite southSprite;

    [Header("Biology")]
    public BodySex bodySex = BodySex.Both;
    public bool hybridable = false;
}
