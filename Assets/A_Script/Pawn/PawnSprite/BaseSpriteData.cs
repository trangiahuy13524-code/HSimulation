using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseSpriteData : BaseOffset
{
    [SerializeField] protected SpritePart spriteDirectionData;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Pawn pM;
    [SerializeField] protected List<BaseOffset> children = new List<BaseOffset>();
    protected int layerOffset = 0;

    public SpritePart SpriteData => spriteDirectionData;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    


    protected Direction currentDirection = Direction.South;

    protected override void Start()
    {
        layerOffset = (World.Instance.WorldSize - 1)*5 + 11;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        base.Start();
    }

    protected void RecalculatePosition()
    {
        CalculatePosition();

        if (children.Count > 0)
        {
            foreach (BaseSpriteData child in children)
            {
                child.CalculatePosition();
            }
        }


    }

    


    protected virtual bool ApplyDirection(Direction dir)
    {
        if (spriteDirectionData == null)
            return false;
        switch (dir)
        {
            case Direction.North:
                spriteRenderer.sprite = spriteDirectionData.northSprite;
                spriteRenderer.flipX = false;
                break;

            case Direction.South:
                spriteRenderer.sprite = spriteDirectionData.southSprite;
                spriteRenderer.flipX = false;
                break;

            case Direction.East:
                spriteRenderer.sprite = spriteDirectionData.eastSprite;
                spriteRenderer.flipX = false;
                break;

            case Direction.West:
                spriteRenderer.sprite = spriteDirectionData.eastSprite;
                spriteRenderer.flipX = true;
                break;
        }
        return true;
    }

    public virtual void SetDirectionSpriteData(SpritePart spriteData)
    {
        spriteDirectionData = spriteData;
        if (initialized)
        {
            RecalculatePosition();
            UpdateFacing();
        }
    }

    public virtual void SetDirection(Direction dir)
    {
        currentDirection = dir;
        UpdateFacing();
    }

    protected virtual void UpdateFacing()
    {
        ApplyDirection(currentDirection);
    }

    public virtual void UpdateLayer()
    {
        spriteRenderer.sortingOrder = layerOffset - pM.CurrentGridPosition.y * 5;
    }
}
