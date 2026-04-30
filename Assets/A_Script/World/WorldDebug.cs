using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class WorldDebug : MonoBehaviour
{
    [SerializeField] World world;
    [SerializeField] List<GeneticData> pawnGeneticsData = new();
    [SerializeField] AutoTillingTile wallTile;
    [SerializeField] Vector2Int spawnPos = Vector2Int.zero;
    [SerializeField] byte spawnCount = 1;
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
            world.GeneratePawn(spawnPosition, pawnGeneticsData[index]);
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

    
}
