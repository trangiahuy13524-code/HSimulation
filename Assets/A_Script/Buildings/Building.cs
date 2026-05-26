using UnityEngine;

public class Building : WorldObject
{

    [SerializeField] protected Transform midPoint;
    [SerializeField] protected BuildingSpriteRender render;
    public Direction direction;
    [SerializeField] public Vector2Int buildingGridSize = Vector2Int.one;
    public BuildingSprite sprite;
    public override bool isPassable => false;
    public override bool canHoldItems => true;

    Vector2Int xRange;
    Vector2Int yRange;
    protected override void Start()
    {
        transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);
        bool isVertical = render.SetDirection(direction);
        if (!isVertical)
        {
            xRange = new Vector2Int(CurrentGridPosition.x, CurrentGridPosition.x + buildingGridSize.x);
            yRange = new Vector2Int(CurrentGridPosition.y, CurrentGridPosition.y + buildingGridSize.y);
        }
        else
        {
            xRange = new Vector2Int(CurrentGridPosition.x, CurrentGridPosition.x + buildingGridSize.y);
            yRange = new Vector2Int(CurrentGridPosition.y, CurrentGridPosition.y + buildingGridSize.x);
        }

        for (int i = xRange.x; i < xRange.y; i++)
            for (int j = yRange.x; j < yRange.y; j++)
                world.RegisterObject(this, new Vector2Int(i, j));

        render.UpdateLayer(world.WorldSize);
    }

    //protected override void OnDestroy()
    //{
    //    for (int i = xRange.x; i < xRange.y; i++)
    //        for (int j = yRange.x; j < yRange.y; j++)
    //            world.FastRemoveObjectData(new Vector2Int(i, j));
    //    base.OnDestroy();
    //}

    public virtual bool checkPlaceable(Vector2Int position, World world)
    {
        bool isVert = checkVert();
        int sizeX = isVert?buildingGridSize.y:buildingGridSize.x;
        int sizeY = isVert ? buildingGridSize.x : buildingGridSize.y;
        Vector2Int tempPos;
        for (int i = position.x; i < position.x + sizeX; i++)
            for (int j = position.y; j < position.y + sizeY; j++)
            {
                tempPos = new Vector2Int(i, j);
                if (!world.IsInside(tempPos)) return false;
                if (world.GetFastObjectAtPosition(tempPos) != null) return false;
                if (world.FastGridMaxPawn(tempPos)) return false;
            }
        return true;
    }

    public bool checkVert()
    {
        switch (direction)
        {
            case Direction.North:
                return false;
            case Direction.South:
                return false;
            case Direction.East:
                return true;
            default:
                return true;
        }
    }

    public override Vector2 GetWorldPos()
    {
        return midPoint.position;
    }
    public override Vector2Int GetMidGrid()
    {
        return WorldUtility.WorldPosToGridPos(midPoint.position);
    }
}