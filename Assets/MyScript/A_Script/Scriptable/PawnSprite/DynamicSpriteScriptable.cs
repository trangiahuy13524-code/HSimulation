using UnityEngine;

public class DynamicSpriteScriptable : SpriteScriptableBase
{
    public SpriteStateData[] spriteTickDatas;
}

public class SpriteStateData
{
    public BodySpritePart spriteData;
    public FacialState facialState;
}