using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/RecipeData")]
public class DataRecipe : DataObjectNeedResearch
{
    public List<ItemDataContainer> requiredItems;
    public List<ItemDataContainer> outputItems;

    public bool useRecipeClass = true;

    public DataItem unfinishedCraftItemData;

    
}