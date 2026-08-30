using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Game/AbilityUI/MoveToObject")]
public class AbilityMoveToObject : DataAbility
{
    public override void Execute(WorldObject caster, Image image = null)
    {
        CameraController.Instance.MoveTo(caster.GetWorldPos());
    }

    public override Sprite GetDefaultIcon(WorldObject caster)
    {
        if (caster != null)
        {
            return caster.IconSprite;
        }
        else
        {
            return icon;
        }
    }
}
