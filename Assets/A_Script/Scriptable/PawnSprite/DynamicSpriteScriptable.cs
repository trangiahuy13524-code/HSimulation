using UnityEngine;

[CreateAssetMenu]
public class DynamicSpriteScriptable : SpriteScriptableBase
{
    public SpriteStateData[] spriteTickDatas;
}

public class SpriteStateData
{
    public DirectionSpriteScriptable spriteData;
    public FacialState facialState;
}