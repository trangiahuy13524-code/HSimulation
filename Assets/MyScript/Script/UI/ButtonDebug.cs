using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonDebug : MonoBehaviour
{
    [SerializeField] Button pawn;
    [SerializeField] Button wall;
    [SerializeField] Button remove;
    [SerializeField] ScreenAndTouchManager screenAndTouchManager;
    [SerializeField] List<DataGenetics> pawnGeneticsData = new();
    [SerializeField] DataWall wallTile;

    void Start()
    {
        if (pawn) pawn.onClick.AddListener(() => {
            Vector2Int spawnPos = screenAndTouchManager.SelectedGrid;
            int index = Random.Range(0, pawnGeneticsData.Count);
            WorldMap.Instance.CreatePawn(spawnPos, pawnGeneticsData[index]);
        });
        if (wall) wall.onClick.AddListener(() => {
            Vector2Int spawnPos = screenAndTouchManager.SelectedGrid;
            WorldMap.Instance.GenerateWall(spawnPos, wallTile);
        });
        if (remove) remove.onClick.AddListener(() => {
            Vector2Int removePos = screenAndTouchManager.SelectedGrid;
            WorldMap.Instance.RemoveObject(removePos);
        });
    }
}
