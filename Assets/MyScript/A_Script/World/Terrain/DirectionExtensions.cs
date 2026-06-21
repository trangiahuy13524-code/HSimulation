using UnityEngine;

public static class DirectionExtensions
{
    public static Vector2Int Position(this TileDirection direction)
    {
        switch (direction)
        {
            case TileDirection.S:
                return new Vector2Int(0, -1);
            case TileDirection.SW:
                return new Vector2Int(-1, -1);
            case TileDirection.W:
                return new Vector2Int(-1, 0);
            case TileDirection.NW:
                return new Vector2Int(-1, 1);
            case TileDirection.N:
                return new Vector2Int(0, 1);
            case TileDirection.NE:
                return new Vector2Int(1, 1);
            case TileDirection.E:
                return new Vector2Int(1, 0);
            case TileDirection.SE:
                return new Vector2Int(1, -1);
            default:
                return Vector2Int.zero;
        }
    }
}