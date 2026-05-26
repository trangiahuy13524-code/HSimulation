using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item/ItemOutputData")]
public class ItemOutputData : ScriptableObject
{
    public ItemData itemData;
    public ItemClass itemClass;
    public int amount;
}
