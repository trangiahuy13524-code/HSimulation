using UnityEngine;

public class SpriteAttire : MonoBehaviour
{
    public DataAttire attireData { get; private set; }
    public bool debug { get; private set; }

    [Header("References")]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] PartAttire spriteData;
    [SerializeField] Pawn pawn;
    
    Direction currentDirection = Direction.South;
    Vector2 baseOffset = Vector2.zero;

    int layerIndex = 3;
    public ItemClass itemClass { get; private set; }

    void Start()
    {
        Refresh();
    }
    //protected override int LayerPriority => 3;
    public void Initialize(Pawn pawn, DataAttire attireData, ItemClass itemClass, PartBioSprite bioSpritePart, bool debug)
    {
        this.pawn = pawn;
        this.attireData = attireData;
        spriteData = attireData.GetAttirePart(pawn.Genome.currentBody.bodyPartShape);
        currentDirection = pawn.CurrentDirection;
        this.itemClass = itemClass;
        layerIndex = attireData.bodyTag switch
        {
            BodyTag.Head => 9,
            BodyTag.Torso => 4,
            BodyTag.Legs => 3,
            _ => 9
        };
        if (bioSpritePart) baseOffset = bioSpritePart.attireOffset;
        this.debug = debug;
    }

    public void Refresh()
    {
        ApplyOffset();
        ApplyDirection(currentDirection);
        UpdateLayer();
    }

    void ApplyOffset()
    {
        if (spriteData)
        {
            transform.localScale = spriteData.scale;
            transform.localPosition = spriteData.offset + baseOffset;
            //transform.localPosition = spriteData.offset * transform.localScale / transform.lossyScale + baseOffset;
            //transform.localScale = spriteData.scale * spriteData.scale / transform.lossyScale;

        }
    }

    public void SetSpriteData(PartAttire data)
    {
        spriteData = data;
        Refresh();
    }

    public void SetDirection(Direction dir)
    {
        currentDirection = dir;
        ApplyDirection(dir);
    }

    bool ApplyDirection(Direction dir)
    {
        if (!spriteData) return false;

        return SpriteDirectionApplicator.Apply(
            spriteRenderer,
            spriteData.eastSprite,
            spriteData.northSprite,
            spriteData.southSprite,
            dir);
    }

    public void UpdateLayer()
    {
        WorldData worldData = WorldData.Instance;
        if (worldData == null) return;

        int sortingOrder = worldData.topGridLayer - pawn.CurrentGridPosition.y * worldData.spacing + layerIndex;
        if (spriteRenderer.sortingOrder != sortingOrder)
        {
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }
}
