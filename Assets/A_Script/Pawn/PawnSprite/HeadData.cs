using UnityEngine;

public class HeadData : BaseSpriteData
{
    protected override void Start()
    {
        base.Start();
        layerOffset += 1;
    }

    protected override bool ApplyDirection(Direction dir)
    {
        if (!base.ApplyDirection(dir)) return false;
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
        return true;
    }
}
