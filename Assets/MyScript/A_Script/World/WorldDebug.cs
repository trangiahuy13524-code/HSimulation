using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class WorldDebug : MonoBehaviour
{
    public static WorldDebug Instance { get; private set; }

    public World world;
    public MapRenderer mapRenderer;
    public ScreenAndTouchManager screenAndTouchManager;
    public List<GeneticData> pawnGeneticsData = new();
    public AutoTillingTile wallTile;
    public PlaceableTile tileToPlace;
    public Vector2Int spawnPos = Vector2Int.zero;
    public ItemData debugItem;
    public ItemClass debugItemClass;
    public int debugItemAmount= 1;
    public byte spawnCount = 1;
    public BuildingObject building;
    public Direction buildingDirection = Direction.South;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        screenAndTouchManager = ScreenAndTouchManager.Instance;
        for (int i = 0; i < spawnCount; i++)
        {
            if (world == null) return;
            int count = pawnGeneticsData.Count;
            if (count == 0) return;
            int size = world.WorldSize;
            Vector2Int spawnPosition = new Vector2Int((size + 1)/2 - spawnCount + i*2, (size - 1) / 2);
            int index = Random.Range(0, count);
            world.CreatePawn(spawnPosition, pawnGeneticsData[index]);
        }

        if (wallTile)
        {
            int size = world.WorldSize - 1;
            int mid = size / 2;
            int min = mid - 3;
            int max = mid + 3;
            for (int x = min; x < max + 1; x++)
            {
                world.GenerateWall(new Vector2Int(x, min), wallTile);
                world.GenerateWall(new Vector2Int(x, max), wallTile);
            }
            for (int y = min; y < max + 1; y++)
            {
                world.GenerateWall(new Vector2Int(min, y), wallTile);
                world.GenerateWall(new Vector2Int(max, y), wallTile);
            }
            //world.RemoveObject(new Vector2Int(mid, min));
        }

    }

    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            mapRenderer.map.SetTile(screenAndTouchManager.SelectedGrid, tileToPlace);
        }
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            mapRenderer.map.RevertToTerrainTile(screenAndTouchManager.SelectedGrid);
        }
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            world.CreateBuilding(screenAndTouchManager.SelectedGrid, building, buildingDirection);
        }
        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            world.CreateItem(screenAndTouchManager.SelectedGrid, debugItem, debugItemClass, debugItemAmount);
        }
    }
}
