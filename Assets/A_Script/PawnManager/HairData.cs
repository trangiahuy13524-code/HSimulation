using UnityEngine;

public class HairData : BaseData
{
    protected override void Start()
    {
        base.Start();
        spriteRenderer.sortingOrder = 70;
    }
}
