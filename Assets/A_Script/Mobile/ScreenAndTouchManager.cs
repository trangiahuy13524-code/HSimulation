using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ScreenAndTouchManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI touchPositionText;
    [SerializeField] Camera cam;
    [SerializeField] Transform selectionHighlight;
    [SerializeField] Vector2Int selectedGrid;
    [SerializeField] World world;
    [SerializeField] float zoomSpeed = 0.01f;
    [SerializeField] float minZoom = 4f;
    [SerializeField] float maxZoom = 20f;
    public Vector2Int SelectedGrid => selectedGrid;

    Vector3 dragStartWorldPos;
    bool dragging;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        world = World.Instance;
        worldSize = world.WorldSize;
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
        selectedGrid = new Vector2Int(
            Mathf.RoundToInt(cam.transform.position.x),
            Mathf.RoundToInt(cam.transform.position.y + 0.5f));

        if (selectionHighlight != null)
        {
            selectionHighlight.position =
                new Vector3(selectedGrid.x, selectedGrid.y - 0.5f, selectionHighlight.position.z);

            touchPositionText.text =
                $"Grid Position: {selectedGrid}, pawn count: {world.GetPawnCount(selectedGrid)}";
        }

        if (Touch.activeTouches.Count == 0)
        {
            dragging = false;
            return;
        }

        var touch = Touch.activeTouches[0]; 

        if (IsWorldInputBlocked(touch.screenPosition))
        {
            dragging = false;
            return;
        }

        // =====================
        // PINCH ZOOM
        // =====================
        if (Touch.activeTouches.Count >= 2)
        {
            var touch1 = Touch.activeTouches[1];

            Vector2 prevPos0 = touch.screenPosition - touch.delta;
            Vector2 prevPos1 = touch1.screenPosition - touch1.delta;

            float prevDistance = Vector2.Distance(prevPos0, prevPos1);
            float currentDistance = Vector2.Distance(
                touch.screenPosition,
                touch1.screenPosition);

            float delta = currentDistance - prevDistance;

            // midpoint between fingers
            Vector2 pinchCenter =
                (touch.screenPosition + touch1.screenPosition) * 0.5f;

            HandleZoom(delta, pinchCenter);

            dragging = false;
            return; // IMPORTANT: stop drag logic
        }

        switch (touch.phase)
        {
            case UnityEngine.InputSystem.TouchPhase.Began:
                dragStartWorldPos = cam.ScreenToWorldPoint(
                    new Vector3(
                        touch.screenPosition.x,
                        touch.screenPosition.y,
                        cam.nearClipPlane));

                dragging = true;
                break;

            case UnityEngine.InputSystem.TouchPhase.Moved:
                if (!dragging) return;

                Vector3 currentWorldPos = cam.ScreenToWorldPoint(
                    new Vector3(
                        touch.screenPosition.x,
                        touch.screenPosition.y,
                        cam.nearClipPlane));

                Vector3 difference = dragStartWorldPos - currentWorldPos;

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

    float worldSize;
    void ClampCameraPosition()
    {
        

        Vector3 pos = cam.transform.position;

        pos.x = Mathf.Clamp(pos.x, 0, worldSize - 1);
        pos.y = Mathf.Clamp(pos.y, -.5f, worldSize - 1.5f);

        cam.transform.position = pos;
    }

    bool IsWorldInputBlocked(Vector2 screenPos)
    {
        PointerEventData eventData =
            new PointerEventData(EventSystem.current)
            {
                position = screenPos
            };

        List<RaycastResult> results = new();

        EventSystem.current.RaycastAll(eventData, results);

        foreach (var hit in results)
        {
            var blocker =
                hit.gameObject.GetComponentInParent<IBlocksWorldInput>();

            if (blocker != null && blocker.BlocksWorldInput())
                return true;
        }
        return false;
    }

    void HandleZoom(float delta, Vector2 pinchCenter)
    {
        //1 World position under fingers BEFORE zoom
        Vector3 beforeZoom = cam.ScreenToWorldPoint(
            new Vector3(pinchCenter.x, pinchCenter.y, cam.nearClipPlane));

        //2 Apply zoom
        float newSize = cam.orthographicSize - delta * zoomSpeed;

        cam.orthographicSize =
            Mathf.Clamp(newSize, minZoom, maxZoom);

        //3 World position under fingers AFTER zoom
        Vector3 afterZoom = cam.ScreenToWorldPoint(
            new Vector3(pinchCenter.x, pinchCenter.y, cam.nearClipPlane));

        //4 Move camera so zoom locks to fingers
        cam.transform.position += beforeZoom - afterZoom;

        ClampCameraPosition();
    }
}