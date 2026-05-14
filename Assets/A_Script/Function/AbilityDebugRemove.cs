using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Game/AbilityUI/removeObject")]
public class AbilityDebugRemove : Ability
{
    public override void Execute(WorldObject caster, Image image = null)
    {
        if (caster != null)
        {
            caster.Despawn();
            ScreenAndTouchManager.Instance.RemoveGridAbilities();
        }
    }
}
