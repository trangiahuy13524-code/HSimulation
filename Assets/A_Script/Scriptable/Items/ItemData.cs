using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item", fileName = "NewItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public bool isStackable;
    public int maxStack = 1;
}
