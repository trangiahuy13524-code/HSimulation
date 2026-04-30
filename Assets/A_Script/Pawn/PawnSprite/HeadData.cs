using UnityEngine;

public class HeadData : BaseSpriteData
{

    protected override void ApplyDirection(Direction dir)
    {
        base.ApplyDirection(dir);
        float h = spriteDirectionData.horizontalOffset;
        switch (dir)
        {
            case Direction.East:
                transform.localPosition = offset + new Vector2(h, 0);
                break;
            case Direction.West:
                transform.localPosition = offset - new Vector2(h, 0);
                break;
            default:
                transform.localPosition = offset;
                break;
        }
        
    }
}
