using UnityEngine;
using UnityEngine.UI;

public abstract class DataAbility : DataMain
{
    [SerializeField] protected Sprite icon;

    public abstract void Execute(WorldObject caster, Image image = null);

    public virtual Sprite GetDefaultIcon(WorldObject caster)
    {
        return icon;
    }
}