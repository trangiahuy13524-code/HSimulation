using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Game/AbilityUI/RemoveObject")]
public class AbilityDebugRemove : DataAbility
{
    public override void Execute(WorldObject caster, Image image = null)
    {
        if (caster != null)
        {
            caster.Despawn();
        }
    }
}
