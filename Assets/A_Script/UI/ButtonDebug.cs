using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonDebug : MonoBehaviour
{
    [SerializeField] Button pawn;
    [SerializeField] Button wall;
    [SerializeField] Button remove;
    [SerializeField] ScreenAndTouchManager screenAndTouchManager;
    [SerializeField] List<PawnPreset> pawnPreset = new();
    [SerializeField] AutoTillingTile wallTile;

    void Start()
    {
        if (pawn) pawn.onClick.AddListener(() => {
            Vector2Int spawnPos = screenAndTouchManager.SelectedGrid;
            int index = Random.Range(0, pawnPreset.Count);
            World.Instance.GeneratePawn(spawnPos, pawnPreset[index]);
        });
        if (wall) wall.onClick.AddListener(() => {
            Vector2Int spawnPos = screenAndTouchManager.SelectedGrid;
            World.Instance.GenerateWall(spawnPos, wallTile);
        });
        if (remove) remove.onClick.AddListener(() => {
            Vector2Int removePos = screenAndTouchManager.SelectedGrid;
            World.Instance.RemoveObject(removePos);
        });
    }
}
