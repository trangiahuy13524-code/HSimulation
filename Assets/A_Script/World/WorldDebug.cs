using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class WorldDebug : MonoBehaviour
{
    [SerializeField] World world;
    [SerializeField] PawnPreset bodySprite;
    [SerializeField] PawnPreset headSprite;
    [SerializeField] PawnPreset hairSprite;
    [SerializeField] List<DirectionTexturePreset> pawnPreset = new();
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
    }

    
    void Update()
    {

    }

    
}
