using UnityEngine;

[CreateAssetMenu]
public class BuildingSprite : ScriptableObject
{
    public Vector2 size = Vector2Int.one;
    public Vector2 verticalOffset;
    public Vector2 horizontalOffset;

    public Sprite eastSprite;
    public Sprite northSprite;
    public Sprite southSprite;
}
