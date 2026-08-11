using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class WorldDebug : MonoBehaviour
{

    public WorldMap world;
    public MapRenderer mapRenderer;
    public ObjectSelector objectSelector;
    [Header("Pawn")]
    public List<DataGenetics> pawnGeneticsData = new();
    [Header("Tile")]
    public DataWall wallTile;
    public DataTile tileToPlace;
    [Header("Item")]
    public DataItem debugItem;
    public ItemClass debugItemClass;
    public int debugItemAmount= 1;
    public byte spawnCount = 1;
    [Header("Building")]
    public DataBuilding building;
    public Direction buildingDirection = Direction.South;
    public JobDataWorkable jobToCreate;
    [Header("Attire")]
    public DataAttire debugAttire;
    public ItemClass debugAttireClass;
    public BodyTag debugAttireBodyTagToDrop;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        //if (Keyboard.current.eKey.wasPressedThisFrame)
        //{
        //    mapRenderer.map.SetTile(objectSelector.selectedGrid, tileToPlace);
        //}
        //if (Keyboard.current.rKey.wasPressedThisFrame)
        //{
        //    mapRenderer.map.RevertToTerrainTile(objectSelector.selectedGrid);
        //}
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            world.CreateBuilding(objectSelector.selectedGrid, building, buildingDirection);
        }
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            if (debugItem.IsStackable) world.CreateItem(objectSelector.selectedGrid, debugItem, debugItemClass, debugItemAmount, null);
            else world.CreateItem(objectSelector.selectedGrid, debugItem, debugItemClass, 1, null);
        }
        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            BuildingWorkable buildingWorkable = objectSelector.selectedObject as BuildingWorkable;
            if (buildingWorkable)
            {
                if (jobToCreate != null)
                {
                    buildingWorkable.CreateJob(jobToCreate);
                }
                else
                {
                    Debug.LogWarning("Debug: jobToCreate is null. Please assign a valid job to create.");
                }
            }
        }
        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            Pawn pawn = objectSelector.selectedPawn;
            if (pawn)
            {
                if (pawn.GetAttireSprite(debugAttireBodyTagToDrop) != null)
                {
                    pawn.Undress(debugAttireBodyTagToDrop);
                }
                else
                {
                    if (debugAttire)
                    {
                        pawn.Wear(debugAttire, debugAttireClass, true);
                    }
                }
            
            }
        }
    }
}
