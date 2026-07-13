using UnityEngine;

public class SpriteHead : SpriteBase
{
    protected override int LayerPriority => 5;

    protected override bool ApplyDirection(Direction dir)
    {
        if (!base.ApplyDirection(dir))
        {
            return false;
        }

        float h = spriteData.horizontalOffset;

        switch (dir)
        {
            case Direction.East:
                spriteTransform.localPosition = baseOffset + new Vector2(h, 0);
                break;

            case Direction.West:
                spriteTransform.localPosition = baseOffset - new Vector2(h, 0);
                break;

            default:
                spriteTransform.localPosition = baseOffset;
                break;
        }

        return true;
    }
}