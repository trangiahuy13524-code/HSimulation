using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/DataRecipe")]
public class RecipeData : ObjectDataNeedResearch
{
    public List<ItemDataContainer> requiredItems;
    public List<ItemDataContainer> outputItems;

    public bool useRecipeClass = true;

    public DataItem unfinishedCraftItemData;

    
}