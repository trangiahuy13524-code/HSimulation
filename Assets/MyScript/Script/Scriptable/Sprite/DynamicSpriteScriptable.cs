using UnityEngine;

public class DynamicSpriteScriptable : PartBase
{
    public SpriteStateData[] spriteTickDatas;
}

public class SpriteStateData
{
    public PartBodySprite spriteData;
    public FacialState facialState;
}