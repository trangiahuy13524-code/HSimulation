using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item/ItemSprite")]
public class ItemSprite : ScriptableObject
{
    public Vector2 offset;
    public Vector2 size = Vector2.one;
    public Sprite sprite;
}
