public enum Direction
{
    North,
    South,
    East,
    West
}

public enum TileDirection : ushort
{
    S, SW, W, NW, N, NE, E, SE
}

public enum Sex
{
    None,
    Male,
    Female,
    Hermaphrodite,
    Asexual
}

public enum BodySex
{
    Male,
    Female,
    Both
}

public enum ReproductionType
{
    None,           // Cannot reproduce
    Sexual,         // Male + Female required
    Asexual,        // Self duplication / splitting
    Hermaphroditic  // Either self or partner breeding
}

public enum BirthType
{
    Egg,
    LiveBirth,
    Cloning
}

public enum FacialState
{
    Idle,
    Blink,
    Serious,
    Sleeping,
    Dead
}

public enum ItemClass
{
    Poor,
    Normal,
    Advanced,
    Epic,
    Legendary,
    Ultimate,
    World,
    Universal,
    None
}
public enum Element
{
    Light,
    Darkness,
    Fire,
    Water,
    Earth,
    Wind,
    Lightning,
    Leaf
}
public enum State
{
    Normal,
    Anti
}

public enum ActionResult
{
    Success,
    Cancelled
}

public enum PawnState
{
    Idle,
    Controlled,
    Working,
    Resting,
    Dead
}