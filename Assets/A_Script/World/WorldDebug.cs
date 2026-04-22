using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class WorldDebug : MonoBehaviour
{
    [SerializeField] World world;
    [SerializeField] List<PawnPreset> pawnPreset = new();
    [SerializeField] AutoTillingTile wallTile;
    [SerializeField] Vector2Int spawnPos = Vector2Int.zero;
    [SerializeField] byte spawnCount = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            if (world == null) return;
            int count = pawnPreset.Count;
            if (count == 0) return;
            Vector2Int spawnPosition = new Vector2Int((world.WorldSize + 1)/2 - spawnCount + i*2, 15);
            int index = Random.Range(0, count);
            world.GeneratePawn(spawnPosition, pawnPreset[index]);
        }

        if (wallTile)
        {
            for (int x = 10; x < 21; x++)
            {
                world.GenerateWall(new Vector2Int(x, 12), wallTile);
            }
            for (int y = 12; y < 19; y++)
            {
                world.GenerateWall(new Vector2Int(10, y), wallTile);
            }
            for (int x = 10; x < 21; x++)
            {
                world.GenerateWall(new Vector2Int(x, 18), wallTile);
            }
            for (int y = 12; y < 19; y++)
            {
                world.GenerateWall(new Vector2Int(20, y), wallTile);
            }
            world.RemoveObject(new Vector2Int(15, 12));
        }

    }

    
    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            world.GenerateWall(spawnPos, wallTile);
        }
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            world.RemoveObject(spawnPos);
        }
    }

    
}
