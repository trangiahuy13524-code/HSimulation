using UnityEngine;

public class FacialAnimator
{
    public bool useAnimation;
    [SerializeField] Pawn pawn;
    [SerializeField] FacialSpritePack facialPack;

    [SerializeField] SpriteBase head;
    [SerializeField] SpriteBase eyes;
    [SerializeField] SpriteBase eyesHightlight;
    [SerializeField] SpriteBase brows;
    [SerializeField] SpriteBase mouth;
    [SerializeField] SpriteBase lids;

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
    public PartBodySprite eyes;
    public PartBodySprite eyesHightlight;
    public DynamicSpriteScriptable mouth;
    public DynamicSpriteScriptable lids;
}