using UnityEngine;

public class HeadData : BaseData
{
    public float horizontalOffset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        if (pM == null) pM = transform.parent.parent.GetComponent<Pawn>();
        base.Start();
        spriteRenderer.sortingOrder = 60;

    }

    protected override void ApplyDirection(Direction dir)
    {
        base.ApplyDirection(dir);
        switch (dir)
        {
            case Direction.East:
                transform.localPosition = offset + new Vector2(horizontalOffset, 0);
                break;
            case Direction.West:
                transform.localPosition = offset - new Vector2(horizontalOffset, 0);
                break;
            default:
                transform.localPosition = offset;
                break;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
