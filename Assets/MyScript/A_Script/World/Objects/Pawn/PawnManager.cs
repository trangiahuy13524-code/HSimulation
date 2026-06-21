using System.Collections.Generic;
using UnityEngine;

public class PawnManager : MonoBehaviour
{
    [SerializeField] World world;
    public static PawnManager Instance { get; private set; }

    // Use a list for fast sequential cache-friendly access
    private readonly List<IManagedUpdate> _managedObjects = new List<IManagedUpdate>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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

    // The ONLY native Unity Update loop running in your entire game
    private void Update()
    {
        // Cache count to avoid looking up size every loop iteration
        int count = _managedObjects.Count;
        
        worldTS = GetWTS();
        // Use a standard for-loop (faster than foreach, generates zero garbage collection)
        for (int i = 0; i < count; i++)
        {
            _managedObjects[i].ManagedUpdate(worldTS);
        }
    }

    public static WorldThreadSafe worldTS;
    public WorldThreadSafe GetWTS()
    {
        return new WorldThreadSafe(world.WorldSize, (byte[,])world.pawnCountOnGrid.Clone(), world.MaxPawnCount, (bool[,])world.notPassableTiles.Clone());
    }
}
