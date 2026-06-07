using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class World : MonoBehaviour
{
    public static World Instance;
    [SerializeField] MapRenderer mapRenderer;
    [SerializeField] PlaceableTile wallDummyTile;

    [SerializeField] byte gameFPS = 48;
    [SerializeField] Tilemap terrainTilemap;
    [SerializeField] Tilemap wallTileMap;
    [SerializeField] Transform wallDummies;
    [SerializeField] Wall wallDummyPrefab;
    [SerializeField] Pawn pawnPrefab;
    [SerializeField] Item itemPrefab;
    [SerializeField] Transform cam;
    [SerializeField] short worldSize = 50;

    [SerializeField] byte maxPawnCount = 2;
    WorldObject[,] objects;
    Item[,] items;

    byte[,] pawnCountOnGrid;
    bool[,] notPassableTiles;
    public byte MaxPawnCount => maxPawnCount;
    public bool[,] NotPassableTiles => notPassableTiles;
    public byte[,] PawnCountOnGrid => pawnCountOnGrid;

    public short WorldSize => worldSize;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        Application.targetFrameRate = gameFPS;

        objects = new WorldObject[worldSize, worldSize];
        items = new Item[worldSize, worldSize];
        notPassableTiles = new bool[worldSize, worldSize];

        if (maxPawnCount == 0) maxPawnCount = 1;
        pawnCountOnGrid = new byte[worldSize, worldSize];
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                pawnCountOnGrid[x, y] = 0;
            }
        }
        if (cam) cam.position = new Vector3((worldSize - 1) / 2f, (worldSize - 2) / 2f, cam.position.z);
    }



    static readonly Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };
    public bool IsInside(Vector2Int p)
    {
        return p.x >= 0 && p.y >= 0 &&
               p.x < worldSize && p.y < worldSize;
    }
    public bool FastGridMaxPawn(Vector2Int position)
    {
        if (pawnCountOnGrid[position.x, position.y] >= maxPawnCount) return true;
        return false;
    }
    public bool IsPositionPathValid(Vector2Int position)
    {
        if (pawnCountOnGrid[position.x, position.y] >= maxPawnCount) return false;
        if (notPassableTiles[position.x, position.y]) return false;
        return true;
    }

    public bool IsNotPassable(Vector2Int position)
    {
        return notPassableTiles[position.x, position.y];
    }

    public bool RegisterObject(WorldObject obj, Vector2Int position)
    {
        if (obj == null) return false;
        if (!IsInside(position)) return false;
        if (objects[position.x, position.y] != null) return false;
        objects[position.x, position.y] = obj;
        notPassableTiles[position.x, position.y] = !obj.isPassable;
        return true;
    }
    public void RemoveObject(Vector2Int position)
    {
        if (!IsInside(position)) return;
        WorldObject ob = objects[position.x, position.y];
        if (ob == null) return;
        ob.Despawn();
    }


    public void RegisterItem(Item item, Vector2Int position)
    {
        if (!IsInside(position)) return;
        items[position.x, position.y] = item;
    }
    public void RemoveItem(Vector2Int position)
    {
        if (!IsInside(position)) return;
        Item ob = items[position.x, position.y];
        if (ob == null) return;
        ob.Despawn();
    }
    public void ModifyNotPassableGrid(Vector2Int position, WorldObject @object)
    {
        notPassableTiles[position.x, position.y] = !@object.isPassable;
    }
    public void ResetNotPassableGrid(Vector2Int position)
    {
        notPassableTiles[position.x, position.y] = false;
    }

    public WorldObject GetFastObjectAtPosition(Vector2Int position)
    {
        return objects[position.x, position.y];
    }

    public void SetWallTile(Vector2Int position, Tile tile)
    {
        if (wallTileMap == null) return;
        if (!IsInside(position)) return;
        int x = position.x;
        int y = position.y;
        wallTileMap.SetTile(new Vector3Int(x, y, 0), tile);
        RefreshNeighborWall(x, y);
    }

    public void ModifyPawnCountGrid(Vector2Int position, bool add)
    {
        if (!IsInside(position)) return;
        if (add)
            pawnCountOnGrid[position.x, position.y]++;
        else
            pawnCountOnGrid[position.x, position.y]--;
    }
    public byte GetPawnCount(Vector2Int position)
    {
        if (!IsInside(position)) return 0;
        return pawnCountOnGrid[position.x, position.y];
    }

    public void GenerateWall(Vector2Int position, AutoTillingTile wallTile)
    {
        if (!IsInside(position)) return;
        int x = position.x;
        int y = position.y;
        if (pawnCountOnGrid[x, y] > 0) return;
        if (items[x, y] != null) return;
        WorldObject existingObject = objects[x, y];
        if (objects[x, y] != null) return;
        wallTileMap.SetTile(new Vector3Int(x, y, 0), wallTile);
        RefreshNeighborWall(x, y);
        Wall dummy = Instantiate(wallDummyPrefab, wallDummies);
        dummy.CurrentGridPosition = position;
        //dummy.wallTile = wallTile;
        mapRenderer.map.SetTile(position, wallDummyTile);
    }

    private void RefreshNeighborWall(int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                wallTileMap.RefreshTile(new Vector3Int(x + dx, y + dy, 0));
            }
        }
    }

    Queue<Vector2Int> fnaQueue = new();
    HashSet<Vector2Int> fnaVisited = new();
    public Vector2Int? FindNearestAvailable(Vector2Int start)
    {
        if (!IsInside(start))
            return null;

        fnaQueue.Clear();
        fnaVisited.Clear();

        fnaQueue.Enqueue(start);
        fnaVisited.Add(start);

        while (fnaQueue.Count > 0)
        {
            Vector2Int current = fnaQueue.Dequeue();

            WorldObject obj = objects[current.x, current.y];

            // FOUND EMPTY TILE
            if (obj == null)
                return current;

            // explore neighbours
            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;

                if (!IsInside(next) || objects[next.x, next.y] != null)
                    continue;

                if (!fnaVisited.Add(next))
                    continue;

                fnaQueue.Enqueue(next);
            }
        }
        return null; // no available position
    }

    public Item FindNearestItem(ItemData itemData, ItemClass itemClass, Vector2Int start)
    {
        if (!IsInside(start))
            return null;

        fnaQueue.Clear();
        fnaVisited.Clear();

        fnaQueue.Enqueue(start);
        fnaVisited.Add(start);

        WorldObject obj;
        Item item;

        while (fnaQueue.Count > 0)
        {
            Vector2Int current = fnaQueue.Dequeue();
            
            item = items[current.x, current.y];
            if (item != null && !item.reserved && item.itemData == itemData && item.itemClass == itemClass)
            {
                return item;
            }

            obj = objects[current.x, current.y];
            if (obj != null && !obj.isPassable && obj.CurrentGridPosition != start)
                continue;


            // explore neighbours
            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;

                if (!IsInside(next))
                    continue;

                if (!fnaVisited.Add(next))
                    continue;

                fnaQueue.Enqueue(next);
            }
        }
        return null;
    }

    public Pawn CreatePawn(Vector2Int position, GeneticData geneticData)
    {
        if (pawnPrefab == null || geneticData == null) return null;
        if (!IsInside(position)) return null;
        int x = position.x;
        int y = position.y;
        if (pawnCountOnGrid[x, y] >= maxPawnCount) return null;
        WorldObject existingObject = objects[x, y];
        if (existingObject != null)
        {
            if (!existingObject.isPassable) return null;
        }
        Pawn pawn = Instantiate(pawnPrefab);
        if (pawn == null) return null;
        pawn.CurrentGridPosition = position;
        pawn.transform.position = new Vector3Int(x, y, 0);
        pawn.InitializePawn(geneticData);
        return pawn;
    }

    public void CreateBuilding(Vector2Int position, BuildingObject buildingObject, Direction direction)
    {
        if (buildingObject == null) return;
        buildingObject.building.direction = direction;
        buildingObject.building.CurrentGridPosition = position;
        if (!buildingObject.building.checkPlaceable(position, this)) return;
        Instantiate(buildingObject.building);
    }

    public List<Item> CreateItem( Vector2Int position, ItemData itemData, ItemClass itemClass, int quantity)
    {
        if (itemData == null)
            return null;

        if (quantity <= 0)
            return null;

        if (!IsInside(position))
            return null;

        fnaQueue.Clear();
        fnaVisited.Clear();

        fnaQueue.Enqueue(position);
        fnaVisited.Add(position);

        List<Item> returnedItems = new();
        WorldObject obj;
        Item item;

        while (fnaQueue.Count > 0 && quantity > 0)
        {
            Vector2Int current = fnaQueue.Dequeue();

            obj = objects[current.x, current.y];
            item = items[current.x, current.y];

            if (item == null)
            {
                if (obj != null && !obj.canHoldItems)
                    continue;
                int stackAmount =
                    itemData.isStackable ? Mathf.Min(quantity, itemData.maxStack) : 1;

                Item createdItem = GenerateItem(
                        current,
                        itemData,
                        itemClass,
                        stackAmount);

                returnedItems.Add(createdItem);

                quantity -= stackAmount;
            }
            else if (item != null &&
                     item.reserved == false &&
                     item.itemData == itemData &&
                     item.itemClass == itemClass &&
                     itemData.isStackable)
            {
                int availableSpace = itemData.maxStack - item.StackCount;

                if (availableSpace > 0)
                {
                    int amountToAdd = Mathf.Min(availableSpace, quantity);

                    item.StackCount += amountToAdd;

                    quantity -= amountToAdd;

                    returnedItems.Add(item);
                }
            }

            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;

                if (!IsInside(next))
                    continue;

                if (!fnaVisited.Add(next))
                    continue;

                fnaQueue.Enqueue(next);
            }
        }

        return returnedItems;
    }
    Item GenerateItem(Vector2Int position, ItemData itemData, ItemClass itemClass, int quantity)
    {
        Item item = Instantiate(itemPrefab);
        item.CurrentGridPosition = position;
        item.transform.position = new Vector3Int(position.x, position.y, 0);
        item.SetItemData(itemData, itemClass);
        item.StackCount = quantity;
        return item;
    }
}

public static class WorldUtility
{
    public static Vector2Int WorldPosToGridPos(Vector2 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y + 0.5f));
    }

    public static Vector3 GridToWorld(Vector2Int grid)
    {
        return new Vector3(grid.x, grid.y - 0.5f, 0);
    }
}

public class WorldThreadSafe
{
    public short WorldSize;

    public byte[,] pawnCountOnGrid;
    public bool[,] notPassableTiles;
    public byte maxPawnCount;

    public WorldThreadSafe(short worldSize, byte[,] pawnCountOnGrid, byte maxPawnCount, bool[,] notPassableTiles)
    {
        WorldSize = worldSize;

        this.pawnCountOnGrid = pawnCountOnGrid;
        this.notPassableTiles = notPassableTiles;
        this.maxPawnCount = maxPawnCount;
    }

    public bool IsInside(Vector2Int position)
    {
        return position.x >= 0 &&
               position.y >= 0 &&
               position.x < WorldSize &&
               position.y < WorldSize;
    }

    public bool IsPositionPathValid(Vector2Int position)
    {
        bool isMaxPawn = pawnCountOnGrid[position.x, position.y] >= maxPawnCount;
        return !isMaxPawn && !notPassableTiles[position.x, position.y];
    }

    public bool IsNotPassable(Vector2Int position)
    {
        return notPassableTiles[position.x, position.y];
    }
}