using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Game/AbilityUI/controlPawn")]
public class AbilityControlPawn : Ability
{
    [SerializeField] Sprite onControl;

    public override void Execute(WorldObject caster, Image image = null)
    {
        Pawn pawn = caster as Pawn;
        if (pawn == null) return;
        if (pawn.isControlled)
        {
            pawn.StopControlPawn();
            image.sprite = icon;
        }
        else
        {
            pawn.ControlPawn();
            image.sprite = onControl;
        }
    }

    public override Sprite GetDefaultIcon(WorldObject caster)
    {
        Pawn p = caster as Pawn;
        if (p != null)
        {
            if (p.isControlled)
            {
                return onControl;
            }
            else
            {
                return icon;
            }
        }
        else
        {
            return icon;
        }
    }
}
