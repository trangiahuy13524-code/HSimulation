using System.Collections.Generic;
using UnityEngine;

public partial class Pawn
{
    [Header("Attire")]
    [SerializeField] SpriteAttire attirePrefab;
    private readonly Dictionary<BodyTag, SpriteAttire> attireSprites = new();

    public bool Wear(Item item)
    {
        if (item == null) return false;
        DataAttire attireData = item.itemData as DataAttire;

        if (!Wear(attireData, item.itemClass)) return false;

        item.Despawn();
        return true;
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
        attireSprites.TryGetValue(bodyTag, out SpriteAttire attireSprite);
        return attireSprite;
    }

    public bool Undress(BodyTag bodyTag)
    {
        if (!attireSprites.TryGetValue(bodyTag, out SpriteAttire attireSprite) || !attireSprite)
        {
            return false;
        }

        attireSprites.Remove(bodyTag);
        if (!attireSprite.debug)
        {
            world.CreateItem(currentGridPos, attireSprite.attireData, attireSprite.itemClass, 1, null);
        }

        Destroy(attireSprite.gameObject);
        return true;
    }

    void ChangeAttireDirection(Direction dir)
    {
        foreach (SpriteAttire attire in attireSprites.Values)
        {
            if (attire)
            {
                attire.SetDirection(dir);
            }
        }
    }

    void UpdateAttireLayer()
    {
        foreach (SpriteAttire attire in attireSprites.Values)
        {
            if (attire)
            {
                attire.UpdateLayer();
            }
        }
    }

    void CreateAttireSprite(DataAttire attire, ItemClass itemClass, BodyTag bodyTag, bool debug)
    {
        SpriteAttire attireSprite = null;
        Transform parent = bodyTag switch
        {
            BodyTag.OffBody => transform,
            BodyTag.Head => headData?.spriteTransform,
            BodyTag.Torso => bodyData.spriteTransform,
            BodyTag.Legs => bodyData.spriteTransform,
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
