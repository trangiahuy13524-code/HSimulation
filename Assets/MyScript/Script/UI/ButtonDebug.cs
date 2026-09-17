using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.LightTransport;
using UnityEngine.UI;

public class ButtonDebug : MonoBehaviour
{
    [SerializeField] Button pawn;
    [SerializeField] Button wall;
    [SerializeField] Button item;
    [SerializeField] WorldDebug worldDB;
    [SerializeField] WorldMap world;
    [SerializeField] ScreenAndTouchManager screenAndTouchManager;
    [SerializeField] List<DataGenetics> pawnGeneticsData = new();
    [SerializeField] DataWall wallTile;

    void Start()
    {
        if (pawn) pawn.onClick.AddListener(() => {
            Vector2Int spawnPos = screenAndTouchManager.SelectedGrid;
            int index = Random.Range(0, pawnGeneticsData.Count);
            world.CreatePawn(spawnPos, pawnGeneticsData[index]);
        });
        if (wall) wall.onClick.AddListener(() => {
            Vector2Int spawnPos = screenAndTouchManager.SelectedGrid;
            world.GenerateWall(spawnPos, wallTile);
        });
        if (item) item.onClick.AddListener(() => {
            Vector2Int itemPos = screenAndTouchManager.SelectedGrid;
            if (worldDB.debugItem.IsStackable) world.CreateItem(itemPos, worldDB.debugItem, worldDB.debugItemClass, worldDB.debugItemAmount, null);
            else world.CreateItem(itemPos, worldDB.debugItem, worldDB.debugItemClass, 1, null);
        });
    }
}
