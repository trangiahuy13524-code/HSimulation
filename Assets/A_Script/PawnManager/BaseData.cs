using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseData : MonoBehaviour
{
    [SerializeField] protected DirectionSpriteData spriteDirectionData;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Pawn pM;
    [SerializeField] protected List<BaseData> children = new List<BaseData>();
    [SerializeField] protected BaseData parent;

    protected Vector2 offset = Vector2.zero;
    protected bool initialized = false;
    protected Direction currentDirection = Direction.South;

    protected virtual void Start()
    {
        
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        CalculatePosition();

        initialized = true;
    }

    private void RecalculatePosition()
    {
        CalculatePosition();

        if (children.Count > 0)
        {
            foreach (BaseData child in children)
            {
                child.CalculatePosition();
            }
        }

        
    }

    private void CalculatePosition()
    {
        if (parent)
        {
            if (parent.spriteDirectionData) offset += parent.spriteDirectionData.childOffset;
        }
        if (spriteDirectionData)
        {
            offset += spriteDirectionData.offset;
            transform.localScale = spriteDirectionData.scale;
            spriteRenderer.sprite = spriteDirectionData.southSprite;
        }
        transform.localPosition = offset;
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

    public virtual void SetDirectionSpriteData(DirectionSpriteData spriteData)
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
