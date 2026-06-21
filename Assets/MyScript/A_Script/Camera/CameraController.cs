using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    void Awake()
    {
        Instance = this;
    }

    public void MoveTo(Vector2 worldPos)
    {
        transform.position =
            new Vector3(worldPos.x, worldPos.y, transform.position.z);
    }
}