using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item/Attire/AttirePart")]
public class PartAttire : ScriptableObject
{
    public Vector2 offset;
    public Vector2 scale = Vector2.one;
    public ShapeBodyPart bodyPartShape;

    public Sprite eastSprite;
    public Sprite northSprite;
    public Sprite southSprite;
}
