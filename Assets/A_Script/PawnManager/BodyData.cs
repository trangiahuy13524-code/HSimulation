using UnityEngine;

public class BodyData : BaseData
{

    protected override void Start()
    {
        base.Start();
        spriteRenderer.sortingOrder = 10;
    }
    
}