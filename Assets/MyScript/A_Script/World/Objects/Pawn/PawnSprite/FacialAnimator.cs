using UnityEngine;

public class FacialAnimator : BaseOffset
{
    public bool useAnimation;
    [SerializeField] Pawn pawn;
    [SerializeField] FacialSpritePack facialPack;

    [SerializeField] BaseSpriteData head;
    [SerializeField] BaseSpriteData eyes;
    [SerializeField] BaseSpriteData eyesHightlight;
    [SerializeField] BaseSpriteData brows;
    [SerializeField] BaseSpriteData mouth;
    [SerializeField] BaseSpriteData lids;

    public void SetPack(FacialSpritePack pack)
    {
        facialPack = pack;
    }

    public void SetDirection(Direction dir)
    {
        eyes.SetDirection(dir);
        eyesHightlight.SetDirection(dir);
    }

    //private void Update()
    //{
    //    if (!useAnimation) return;
    //}
}

public class FacialSpritePack
{
    public DynamicSpriteScriptable head;
    public DynamicSpriteScriptable brows;
    public BodySpritePart eyes;
    public BodySpritePart eyesHightlight;
    public DynamicSpriteScriptable mouth;
    public DynamicSpriteScriptable lids;
}