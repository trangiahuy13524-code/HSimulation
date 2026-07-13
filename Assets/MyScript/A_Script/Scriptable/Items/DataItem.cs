using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item/Normal/ItemData")]
public class DataItem : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [SerializeField] bool isStackable;
    public int maxStack = 1;
    public ItemSprite itemSprite;


    public virtual bool IsStackable => isStackable;
}
