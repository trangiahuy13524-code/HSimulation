using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseSpriteData : BaseOffset
{
    [SerializeField] protected DirectionSpriteScriptable spriteDirectionData;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Pawn pM;
    [SerializeField] protected List<BaseOffset> children = new List<BaseOffset>();

    public DirectionSpriteScriptable SpriteData => spriteDirectionData;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    


    protected Direction currentDirection = Direction.South;

    protected override void Start()
    {
        
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

    


    protected virtual void ApplyDirection(Direction dir)
    {
        if (spriteDirectionData == null)
            return;
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
    }

    public virtual void SetDirectionSpriteData(DirectionSpriteScriptable spriteData)
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
}
