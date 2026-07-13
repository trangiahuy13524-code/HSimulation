using UnityEngine;

public class BuildingSpriteRender : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Building building;
    int layerOffset;

    private void Start()
    {
        transform.localScale = new Vector3(building.sprite.size.x, building.sprite.size.y, 1);
    }

    public bool SetDirection(Direction direction)
    {
        bool vert = false;
        switch (direction)
        {
            case Direction.North:
                spriteRenderer.sprite = building.sprite.northSprite;
                vert = false;
                break;
            case Direction.South:
                spriteRenderer.sprite = building.sprite.southSprite;
                vert = false;
                break;
            case Direction.East:
                spriteRenderer.sprite= building.sprite.eastSprite;
                spriteRenderer.flipX = false;
                vert = true;
                break;
            default:
                spriteRenderer.sprite = building.sprite.eastSprite;
                spriteRenderer.flipX = true;
                vert = true;
                break;
        }
        if (vert)
        {
            transform.localPosition = new Vector3(building.sprite.verticalOffset.x, building.sprite.verticalOffset.y, 0);
        }
        else
        {
            transform.localPosition = new Vector3(building.sprite.horizontalOffset.x, building.sprite.horizontalOffset.y, 0);
        }
        return vert;
    }

    public virtual void UpdateLayer()
    {
        bool isVertical = building.checkVert(building.direction);
        int offset = 0;
        if (isVertical)
        {
            offset = building.buildingGridSize.x - 1;
        }
        else
        {
            offset = building.buildingGridSize.y - 1;
        }
        spriteRenderer.sortingOrder = WorldStatic.Instance.topGridLayer - (building.CurrentGridPosition.y + offset) * WorldStatic.Instance.spacing;
    }
}
