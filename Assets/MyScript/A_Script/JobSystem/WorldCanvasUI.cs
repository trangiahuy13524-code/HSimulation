using UnityEngine;

public class WorldCanvasUI : MonoBehaviour
{
    public static WorldCanvasUI Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }
}
