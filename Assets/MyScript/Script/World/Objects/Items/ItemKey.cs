using UnityEngine;
using System;

[Serializable]
public struct ItemKey
{
    public DataItem itemData;
    public ItemClass itemClass;

    public ItemKey(DataItem data, ItemClass itemClass)
    {
        this.itemData = data;
        this.itemClass = itemClass;
    }

    public override bool Equals(object obj)
    {
        if (obj is not ItemKey other)
            return false;

        return itemData == other.itemData &&
               itemClass == other.itemClass;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(itemData, itemClass);
    }
}