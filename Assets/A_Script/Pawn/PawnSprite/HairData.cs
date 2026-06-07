 using UnityEngine;

public class HairData : BaseSpriteData
{
    protected override void Start()
    {
        base.Start();
        layerOffset += 2;
    }
}