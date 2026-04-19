using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class WorldDebug : MonoBehaviour
{
    [SerializeField] World world;
    [SerializeField] DirectionSpriteData bodySprite;
    [SerializeField] DirectionSpriteData headSprite;
    [SerializeField] DirectionSpriteData hairSprite;
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
                world.GenerateWall(wallTile, new Vector2Int(x, 12));
            }
            for (int y = 10; y < 21; y++)
            {
                world.GenerateWall(wallTile, new Vector2Int(10, y));
            }
            for (int x = 10; x < 21; x++)
            {
                world.GenerateWall(wallTile, new Vector2Int(x, 18));
            }
            for (int y = 10; y < 21; y++)
            {
                world.GenerateWall(wallTile, new Vector2Int(20, y));
            }
            world.RemoveWall(new Vector2Int(15, 12));
        }

    }

    
    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            world.GenerateWall(wallTile, spawnPos);
        }
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            world.RemoveWall(spawnPos);
        }
    }

    
}
