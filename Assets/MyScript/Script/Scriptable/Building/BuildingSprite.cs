using UnityEngine;

[CreateAssetMenu(menuName = "Game/Building/BuildingSprite")]
public class BuildingSprite : ScriptableObject
{
    public Vector2 size = Vector2Int.one;
    public bool fixedSouth = false;
    public Vector2 horizontalOffset;
    public Vector2 verticalOffset;
    public Sprite icon;
    public Sprite eastSprite;
    public Sprite northSprite;
    public Sprite southSprite;
}
