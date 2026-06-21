using UnityEngine;

public class LastFrameUpdate : MonoBehaviour
{
    public static LastFrameUpdate Instance { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        Pawn.aPawnThoughtThisFrame = false;
    }
}
