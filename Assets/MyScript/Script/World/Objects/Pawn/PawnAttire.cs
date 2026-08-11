using System.Collections.Generic;
using UnityEngine;

public partial class Pawn
{
    [Header("Attire")]
    [SerializeField] SpriteAttire attirePrefab;
    Dictionary<BodyTag, SpriteAttire> attireSprites = new();

    public bool Wear(Item item)
    {
        if (item == null) return false;
        DataAttire attireData = item.itemData as DataAttire;

        if (Wear(attireData, item.itemClass))
        {
            item.Despawn();
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool Wear(DataAttire attireData, ItemClass itemClass, bool debug = false)
    {
        if (attireData == null) return false;
        if (attireData.attirePart.bodyPartShape != null && genome.currentBody.bodyPartShape != attireData.attirePart.bodyPartShape)
        {
            Debug.LogWarning($"class PawnAttire line 34: Failed to wear item {attireData.thingName} due to body part shape mismatch");
            return false;
        }
        CreateAttireSprite(attireData, itemClass, attireData.bodyTag, debug);
        return true;
    }

    public SpriteAttire GetAttireSprite(BodyTag bodyTag)
    {
        if (attireSprites.ContainsKey(bodyTag))
        {
            return attireSprites[bodyTag];
        }
        return null;
    }

    public bool Undress(BodyTag bodyTag)
    {
        if (!attireSprites.ContainsKey(bodyTag) || !attireSprites[bodyTag])
        {
            return false;
        }
        SpriteAttire attireSprite = attireSprites[bodyTag];
        if (attireSprite != null)
        {
            attireSprites.Remove(bodyTag);
            if (!attireSprite.debug) world.CreateItem(currentGridPos, attireSprite.attireData, attireSprite.itemClass, 1, null);
            Destroy(attireSprite.gameObject);
            return true;
        }
        return false;
    }

    void ChangeAttireDirection(Direction dir)
    {
        foreach (var attire in attireSprites)
        {
            if (attire.Value)
            {
                attire.Value.SetDirection(dir);
            }
        }
    }

    void UpdateAttireLayer()
    {
        foreach (var attire in attireSprites)
        {
            if (attire.Value)
            {
                attire.Value.UpdateLayer();
            }
        }
    }

    void CreateAttireSprite(DataAttire attire, ItemClass itemClass, BodyTag bodyTag, bool debug)
    {
        SpriteAttire attireSprite = null;
        Transform parent = bodyTag switch
        {
            BodyTag.OffBody => transform,
            BodyTag.Head => headData?.spriteTransform.transform,
            BodyTag.Torso => bodyData.spriteTransform.transform,
            BodyTag.Legs => bodyData.spriteTransform.transform,
            _ => null
        };

        PartBioSprite parentPart = bodyTag switch
        {
            BodyTag.Head => headData?.SpritePart,
            BodyTag.Torso => bodyData.SpritePart,
            BodyTag.Legs => bodyData.SpritePart,
            _ => null
        };
        if (parent != null)
        {
            attireSprite = Instantiate(attirePrefab, parent);
            attireSprite.Initialize(this, attire, itemClass, parentPart, debug);
        }
        attireSprites[bodyTag] = attireSprite;
    }
}
