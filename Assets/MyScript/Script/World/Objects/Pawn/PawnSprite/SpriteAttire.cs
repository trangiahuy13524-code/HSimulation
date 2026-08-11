using UnityEngine;

public class SpriteAttire : MonoBehaviour
{
    private WorldData worldData;
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
        worldData = WorldData.Instance;
        Refresh();
    }
    //protected override int LayerPriority => 3;
    public void Initialize(Pawn pawn, DataAttire attireData, ItemClass itemClass, PartBioSprite bioSpritePart, bool debug)
    {
        this.pawn = pawn;
        this.attireData = attireData;
        spriteData = attireData.attirePart;
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
            transform.localPosition = spriteData.offset + baseOffset;
            transform.localScale = spriteData.scale;
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

    public void UpdateLayer()
    {
        if (worldData == null) return;
        spriteRenderer.sortingOrder = worldData.topGridLayer - pawn.CurrentGridPosition.y * worldData.spacing + layerIndex;
    }
}