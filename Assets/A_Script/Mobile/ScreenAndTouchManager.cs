using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ScreenAndTouchManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI touchPositionText;
    [SerializeField] Camera cam;
    [SerializeField] Transform selectionHighlight;
    [SerializeField] Vector2Int selectedGrid;
    [SerializeField] World world;
    public Vector2Int SelectedGrid => selectedGrid;

    Vector3 dragStartWorldPos;
    bool dragging;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        world = World.Instance;
    }
    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        selectedGrid = new Vector2Int(Mathf.RoundToInt(cam.transform.position.x + 0.5f), Mathf.RoundToInt(cam.transform.position.y + 0.5f));
        if (selectionHighlight != null)
        {
            selectionHighlight.position = new Vector3(selectedGrid.x, selectedGrid.y - 0.5f, selectionHighlight.position.z);
            touchPositionText.text = $"Grid Position: {selectedGrid}, pawn count: {world.GetPawnCount(selectedGrid)}";
        }
        //touchPositionText.text = $"Grid Position: {gridPos}";

        if (Touch.activeTouches.Count == 0)
        {
            dragging = false;
            return;
        }
        var touch = Touch.activeTouches[0];

        switch (touch.phase)
        {
            // Finger pressed
            case UnityEngine.InputSystem.TouchPhase.Began:
                dragStartWorldPos =
                    cam.ScreenToWorldPoint(
                        new Vector3(
                            touch.screenPosition.x,
                            touch.screenPosition.y,
                            cam.nearClipPlane));

                dragging = true;
                break;

            // Finger moving
            case UnityEngine.InputSystem.TouchPhase.Moved:
                if (!dragging) return;

                Vector3 currentWorldPos =
                    cam.ScreenToWorldPoint(
                        new Vector3(
                            touch.screenPosition.x,
                            touch.screenPosition.y,
                            cam.nearClipPlane));

                Vector3 difference =
                    dragStartWorldPos - currentWorldPos;

                cam.transform.position += difference;
                ClampCameraPosition();
                break;

            case UnityEngine.InputSystem.TouchPhase.Ended:
            case UnityEngine.InputSystem.TouchPhase.Canceled:
                dragging = false;
                break;
        }
    }

    Vector2Int ScreenToGridPosition(Vector2 screenPos)
    {
        Vector2 worldPos = cam.ScreenToWorldPoint(screenPos);
        return new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y + 0.5f));
    }
    void ClampCameraPosition()
    {
        float min = -0.5f;
        float max = World.Instance.WorldSize - 1.5f;

        Vector3 pos = cam.transform.position;

        pos.x = Mathf.Clamp(pos.x, min, max);
        pos.y = Mathf.Clamp(pos.y, min, max);

        cam.transform.position = pos;
    }

    void CheckPawnAtScreenCenter()
    {
        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        Vector2 worldPoint = cam.ScreenToWorldPoint(center);

        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (hit.collider != null)
        {
            Pawn pawn = hit.collider.GetComponent<Pawn>();

            if (pawn != null)
                Debug.Log("Selected Pawn: " + pawn.name);
        }
    }
}