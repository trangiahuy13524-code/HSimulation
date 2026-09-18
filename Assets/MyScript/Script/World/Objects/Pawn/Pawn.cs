using TMPro;
using UnityEngine;

public partial class Pawn : WorldObjectDynamic
{
    private const float WORLD_POSITION_Y_OFFSET = 0.5f;

    [Header("Pawn Appearance")]
    [SerializeField] SpriteBody bodyData;
    [SerializeField] SpriteHead headData;
    [SerializeField] SpriteHair hairData;
    [SerializeField] Direction oldDirection = Direction.South;
    [SerializeField] TextMeshPro displayTextName;
    [SerializeField] Rigidbody2D rb;

    public Direction CurrentDirection => oldDirection;
    public SpriteBody BodyData => bodyData;
    public SpriteHead HeadData => headData;
    public SpriteHair HairData => hairData;
    public Vector2 CurrentWorldPos => rb.position;

    public override Vector2 GetWorldPos()
    {
        return new Vector2(rb.position.x, rb.position.y - WORLD_POSITION_Y_OFFSET);
    }

    public Vector2Int direction()
    {
        return oldDirection switch
        {
            Direction.North => Vector2Int.up,
            Direction.South => Vector2Int.down,
            Direction.East => Vector2Int.right,
            Direction.West => Vector2Int.left,
            _ => Vector2Int.zero
        };
    }

    public void ChangeDirection(Direction dir)
    {
        if (dir == oldDirection) return;

        oldDirection = dir;
        if (bodyData) bodyData.SetDirection(dir);
        if (headData) headData.SetDirection(dir);
        if (hairData) hairData.SetDirection(dir);
        ChangeAttireDirection(dir);
    }

    public override void UpdateLayer()
    {
        if (bodyData) bodyData.UpdateLayer();
        if (headData) headData.UpdateLayer();
        if (hairData) hairData.UpdateLayer();
        UpdateAttireLayer();
    }
}
