using UnityEngine;

public class HairData : BaseData
{
    protected override void Start()
    {
        if (pM == null) pM = transform.parent.parent.parent.GetComponent<Pawn>();
        base.Start();
        spriteRenderer.sortingOrder = 12;
    }
}
