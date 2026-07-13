using System.Collections.Generic;
using UnityEngine;

public abstract class SpriteBase : MonoBehaviour
{
    private WorldStatic worldStatic;
    public PartBioSprite SpritePart => spriteData;
    [Header("References")]
    [SerializeField] protected Transform spriteTransform;
    [SerializeField] protected SpriteBase parent;
    [SerializeField] protected List<SpriteBase> children = new();

    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected PartBioSprite spriteData;
    [SerializeField] protected Pawn pawn;

    protected Vector2 baseOffset;
    protected Direction currentDirection = Direction.South;

    protected virtual int LayerPriority => 0;

    protected virtual void Start()
    {
        worldStatic = WorldStatic.Instance;
        Refresh();
    }

    public void Refresh()
    {
        ApplyOffset();
        ApplyDirection(currentDirection);
        UpdateLayer();

        foreach (var child in children)
            child.Refresh();
    }

    protected virtual void ApplyOffset()
    {
        baseOffset = Vector2.zero;

        if (parent && parent.spriteData)
            baseOffset += parent.spriteData.childOffset;

        if (spriteData)
        {
            baseOffset += spriteData.offset;
            spriteTransform.localScale = spriteData.scale;
        }

        spriteTransform.localPosition = baseOffset;
    }

    public virtual void SetSpriteData(PartBioSprite data)
    {
        spriteData = data;
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

        switch (dir)
        {
            case Direction.North:
                spriteRenderer.sprite = spriteData.northSprite;
                spriteRenderer.flipX = false;
                break;

            case Direction.South:
                spriteRenderer.sprite = spriteData.southSprite;
                spriteRenderer.flipX = false;
                break;

            case Direction.East:
                spriteRenderer.sprite = spriteData.eastSprite;
                spriteRenderer.flipX = false;
                break;

            case Direction.West:
                spriteRenderer.sprite = spriteData.eastSprite;
                spriteRenderer.flipX = true;
                break;
        }

        return true;
    }

    public virtual void UpdateLayer()
    {
        if (worldStatic == null) return;
        spriteRenderer.sortingOrder = worldStatic.topGridLayer - pawn.CurrentGridPosition.y * worldStatic.spacing + LayerPriority;
    }

    public virtual void SetMaterial(Material mat)
    {
        spriteRenderer.sharedMaterial = mat;
    }
}