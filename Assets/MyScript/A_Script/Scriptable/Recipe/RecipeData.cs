using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/RecipeData")]
public class RecipeData : ScriptableObjectNeedResearch
{
    public List<ItemDataContainer> requiredItems;
    public List<ItemDataContainer> outputItems;

    public bool useRecipeClass = true;

    public ItemData unfinishedCraftItemData;

    
}