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
            int size = world.WorldSize;
            int min = size / 4;
            int max = size * 3 / 4 + 1;
            for (int x = min; x < max; x++)
            {
                world.GenerateWall(new Vector2Int(x, min + 3), wallTile);
            }
            for (int y = min + 3; y < max - 3; y++)
            {
                world.GenerateWall(new Vector2Int(min, y), wallTile);
            }
            for (int x = min; x < max; x++)
            {
                world.GenerateWall(new Vector2Int(x, max - 4), wallTile);
            }
            for (int y = min + 3; y < max - 3; y++)
            {
                world.GenerateWall(new Vector2Int(max - 1, y), wallTile);
            }
            world.RemoveObject(new Vector2Int(size / 2, min + 3));
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
