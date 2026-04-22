using UnityEngine;

public enum Direction
{
    North,
    South,
    East,
    West
}

public enum DiagonalDirection
{
    North,
    NorthEast,
    East,
    SouthEast,
    South,
    SouthWest,
    West,
    NorthWest
}


public enum TerrainType
{
    Water, Sand, Grass, Rocks
}
public enum TileDirection : ushort
{
    S, SW, W, NW, N, NE, E, SE
}