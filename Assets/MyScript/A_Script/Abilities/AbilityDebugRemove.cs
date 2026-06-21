using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Game/AbilityUI/RemoveObject")]
public class AbilityDebugRemove : Ability
{
    public override void Execute(WorldObject caster, Image image = null)
    {
        if (caster != null)
        {
            caster.Despawn();
        }
    }
}
