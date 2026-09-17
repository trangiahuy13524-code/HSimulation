using UnityEngine;

public class SpriteHead : SpriteBase
{
    protected override int LayerPriority => 5;
    protected override bool InheritParentScale => false;

    protected override void ApplyDirectionalOffset(Direction dir)
    {

        switch (dir)
        {
            case Direction.East:
                transform.localPosition = baseOffset + new Vector2(baseHorizontalOffset, 0);
                break;

            case Direction.West:
                transform.localPosition = baseOffset - new Vector2(baseHorizontalOffset, 0);
                break;

            default:
                transform.localPosition = baseOffset;
                break;
        }
    }
}
