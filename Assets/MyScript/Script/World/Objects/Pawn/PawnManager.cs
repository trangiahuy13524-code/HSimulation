using System;
using System.Collections.Generic;
using UnityEngine;

public class PawnManager : MonoBehaviour
{
    [SerializeField] WorldMap world;
    public static PawnManager Instance { get; private set; }

    private readonly List<IManagedUpdate> _managedObjects = new();

    // Double-buffer snapshots
    private WorldThreadSafe readBuffer;
    private WorldThreadSafe writeBuffer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        readBuffer = CreateBuffer();
        writeBuffer = CreateBuffer();
    }

    private WorldThreadSafe CreateBuffer()
    {
        return new WorldThreadSafe(
            world.WorldSize,
            new byte[world.WorldSize, world.WorldSize],
            world.MaxPawnCount,
            new bool[world.WorldSize, world.WorldSize]
        );
    }

    public static void Register(IManagedUpdate obj)
    {
        if (Instance != null && !Instance._managedObjects.Contains(obj))
        {
            Instance._managedObjects.Add(obj);
        }
    }

    public static void Unregister(IManagedUpdate obj)
    {
        if (Instance != null)
        {
            Instance._managedObjects.Remove(obj);
        }
    }

    private void Update()
    {
        // Fill write buffer
        FillBuffer(writeBuffer);

        // Swap buffers
        (readBuffer, writeBuffer) = (writeBuffer, readBuffer);

        // Update all pawns using frozen snapshot
        int count = _managedObjects.Count;
        for (int i = 0; i < count; i++)
        {
            _managedObjects[i].ManagedUpdate(readBuffer);
        }
    }

    public WorldThreadSafe GetWTS()
    {
        // Return frozen snapshot (safe for threads)
        return readBuffer;
    }

    private void FillBuffer(WorldThreadSafe buffer)
    {
        Array.Copy(
            world.pawnCountOnGrid,
            buffer.pawnCountOnGrid,
            world.pawnCountOnGrid.Length
        );

        Array.Copy(
            world.notPassableTiles,
            buffer.notPassableTiles,
            world.notPassableTiles.Length
        );
    }
}