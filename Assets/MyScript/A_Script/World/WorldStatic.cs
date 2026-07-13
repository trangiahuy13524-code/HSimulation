using UnityEngine;

public class WorldStatic : MonoBehaviour
{
    [Header("LayerIndex")]
    public int bottomGridLayer = 1000;
    public int spacing = 10;
    [Header("Ref")]
    public int topGridLayer = 1000;

    [Header("Shader")]
    public Material defaultMat;
    public Material hoverMat;
    public Material selectedMat;
    public static WorldStatic Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        topGridLayer = WorldMap.Instance.WorldSize * spacing + bottomGridLayer;
    }
}
