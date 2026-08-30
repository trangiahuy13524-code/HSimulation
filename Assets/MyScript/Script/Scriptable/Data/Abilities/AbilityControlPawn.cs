using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Game/AbilityUI/ControlPawn")]
public class AbilityControlPawn : DataAbility
{
    [SerializeField] Sprite onControl;

    public override void Execute(WorldObject caster, Image image = null)
    {
        Pawn pawn = caster as Pawn;
        if (pawn == null) return;
        if (pawn.PawnState == PawnState.Controlled)
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
            if (p.PawnState == PawnState.Controlled)
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