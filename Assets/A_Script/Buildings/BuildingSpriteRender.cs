using UnityEngine;

public class BuildingSpriteRender : MonoBehaviour
{
    [SerializeField] BuildingSprite sprite;
    [SerializeField] SpriteRenderer spriteRenderer;

    private void Start()
    {
        transform.localScale = new Vector3(sprite.size.x, sprite.size.y, 1);
    }

    public bool SetDirection(Direction direction)
    {
        bool vert = false;
        switch (direction)
        {
            case Direction.North:
                spriteRenderer.sprite = sprite.northSprite;
                vert = false;
                break;
            case Direction.South:
                spriteRenderer.sprite = sprite.southSprite;
                vert = false;
                break;
            case Direction.East:
                spriteRenderer.sprite= sprite.eastSprite;
                spriteRenderer.flipX = false;
                vert = true;
                break;
            default:
                spriteRenderer.sprite = sprite.eastSprite;
                spriteRenderer.flipX = true;
                vert = true;
                break;
        }
        if (vert)
        {
            transform.localPosition = new Vector3(sprite.verticalOffset.x, sprite.verticalOffset.y, 0);
        }
        else
        {
            transform.localPosition = new Vector3(sprite.horizontalOffset.x, sprite.horizontalOffset.y, 0);
        }
        return vert;
    }
}
