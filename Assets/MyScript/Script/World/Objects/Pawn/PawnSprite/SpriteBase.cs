using System.Collections.Generic;
using UnityEngine;

public abstract class SpriteBase : MonoBehaviour
{
    public PartBioSprite SpritePart => spriteData;

    [Header("References")]
    public Transform spriteTransform;
    [SerializeField] protected SpriteBase parent;
    [SerializeField] protected List<SpriteBase> children = new();

    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected PartBioSprite spriteData;
    [SerializeField] protected Pawn pawn;

    protected Vector2 baseOffset;
    protected float baseHorizontalOffset;
    protected Direction currentDirection = Direction.South;

    protected virtual int LayerPriority => 0;
    protected virtual bool InheritParentScale => true;

    public void Refresh()
    {
        RefreshSelf();

        foreach (var child in children)
        {
            child.Refresh();
        }
    }

    private void RefreshSelf()
    {
        ApplyDirection(currentDirection);
        UpdateLayer();
    }

    protected virtual void ApplyOffset()
    {
        baseOffset = Vector2.zero;
        baseHorizontalOffset = 0f;

        if (parent && parent.spriteData)
        {
            baseOffset += parent.spriteData.childOffset;
            baseHorizontalOffset += parent.spriteData.childHorizontalOffset;
        }

        if (spriteData)
        {
            ApplyScale();
            baseOffset += spriteData.offset * transform.localScale / transform.lossyScale;
            baseHorizontalOffset += spriteData.horizontalOffset * transform.localScale.x / transform.lossyScale.x;
        }

        

        transform.localPosition = baseOffset;
    }

    protected virtual void ApplyScale()
    {
        transform.localScale = spriteData.scale;
        if (!InheritParentScale)
        {
            transform.localScale = spriteData.scale * spriteData.scale / transform.lossyScale;
        }
    }

    public virtual void SetSpriteData(PartBioSprite data)
    {
        spriteData = data;
        ApplyOffset();
        Refresh();
    }

    public virtual void SetDirection(Direction dir)
    {
        currentDirection = dir;
        ApplyDirection(dir);
    }

    protected virtual bool ApplyDirection(Direction dir)
    {
        if (!spriteData) return false;

        if (!SpriteDirectionApplicator.Apply(
            spriteRenderer,
            spriteData.eastSprite,
            spriteData.northSprite,
            spriteData.southSprite,
            dir))
        {
            return false;
        }

        ApplyDirectionalOffset(dir);
        return true;
    }

    protected virtual void ApplyDirectionalOffset(Direction dir)
    {
    }

    public virtual void UpdateLayer()
    {
        WorldData worldData = WorldData.Instance;
        if (worldData == null) return;

        int sortingOrder = worldData.topGridLayer - pawn.CurrentGridPosition.y * worldData.spacing + LayerPriority;
        if (spriteRenderer.sortingOrder != sortingOrder)
        {
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }

    public virtual void SetMaterial(Material mat)
    {
        spriteRenderer.sharedMaterial = mat;
    }
}

internal static class SpriteDirectionApplicator
{
    public static bool Apply(
        SpriteRenderer spriteRenderer,
        Sprite eastSprite,
        Sprite northSprite,
        Sprite southSprite,
        Direction direction)
    {
        Sprite sprite;
        bool flipX;

        switch (direction)
        {
            case Direction.North:
                sprite = northSprite;
                flipX = false;
                break;

            case Direction.South:
                sprite = southSprite;
                flipX = false;
                break;

            case Direction.East:
                sprite = eastSprite;
                flipX = false;
                break;

            case Direction.West:
                sprite = eastSprite;
                flipX = true;
                break;

            default:
                return false;
        }

        if (spriteRenderer.sprite != sprite)
        {
            spriteRenderer.sprite = sprite;
        }

        if (spriteRenderer.flipX != flipX)
        {
            spriteRenderer.flipX = flipX;
        }

        return true;
    }
}
