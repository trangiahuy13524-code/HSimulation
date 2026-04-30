using System.Collections.Generic;
using UnityEngine;

public class BaseOffset : MonoBehaviour
{

    [SerializeField] protected BaseOffset parent;
    protected Vector2 offset = Vector2.zero;
    protected bool initialized = false;

    protected virtual void Start()
    {
        CalculatePosition();
        initialized = true;
    }

    protected virtual void CalculatePosition()
    {
        BaseSpriteData parent = this.parent as BaseSpriteData;
        if (parent)
        {
            if (parent.SpriteData) offset += parent.SpriteData.childOffset;
        }
        BaseSpriteData bSD = this as BaseSpriteData;
        if (bSD.SpriteData)
        {
            offset += bSD.SpriteData.offset;
            transform.localScale = bSD.SpriteData.scale;
            bSD.SpriteRenderer.sprite = bSD.SpriteData.southSprite;
        }

        transform.localPosition = offset;
    }
}
