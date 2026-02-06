using UnityEngine;
using UnityEngine.EventSystems;

public class CameraPanController : MonoBehaviour
{
    public Camera cam;

    [Header("Pan")]
    public float panSpeed = 1f;
    public bool invert = true;

    [Header("Input")]
    public bool ignoreWhenPlacingBuilding = true;
    public bool ignoreDragOverUI = true;

    private bool dragging;
    private Vector2 lastScreenPos;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        if (cam == null || GridSystem.instance == null) return;

        if (ignoreWhenPlacingBuilding && BuildingManager.instance != null && BuildingManager.instance.isPlacingBuilding)
        {
            dragging = false;
            return;
        }

        // TOUCH
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);

            if (ignoreDragOverUI && IsPointerOverUI(t.fingerId))
                return;

            if (t.phase == TouchPhase.Began)
            {
                dragging = true;
                lastScreenPos = t.position;
            }
            else if (t.phase == TouchPhase.Moved && dragging)
            {
                // stabil: pixel delta
                Vector2 delta = t.deltaPosition; // vagy: t.position - lastScreenPos
                lastScreenPos = t.position;
                PanByScreenDelta(delta);
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                dragging = false;
            }

            return;
        }

        // MOUSE (Editor)
        if (ignoreDragOverUI && IsPointerOverUI())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            dragging = true;
            lastScreenPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0) && dragging)
        {
            Vector2 current = Input.mousePosition;
            Vector2 delta = current - lastScreenPos;
            lastScreenPos = current;
            PanByScreenDelta(delta);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }
    }

    private void PanByScreenDelta(Vector2 screenDelta)
    {
        // pixel -> world konverzió orthographic kamerához
        float worldPerPixelY = (2f * cam.orthographicSize) / Screen.height;
        float worldPerPixelX = (2f * cam.orthographicSize * cam.aspect) / Screen.width;

        Vector3 move = new Vector3(screenDelta.x * worldPerPixelX, screenDelta.y * worldPerPixelY, 0f);

        if (invert) move = -move;

        cam.transform.position += move * panSpeed;

        ClampCameraToGridBounds();
    }

    private void ClampCameraToGridBounds()
    {
        GridSystem g = GridSystem.instance;

        float minX = g.gridOrigin.x;
        float minY = g.gridOrigin.y;
        float maxX = g.gridOrigin.x + g.gridWidth * g.cellSize;
        float maxY = g.gridOrigin.y + g.gridHeight * g.cellSize;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        float viewW = halfW * 2f;
        float viewH = halfH * 2f;
        float gridW = maxX - minX;
        float gridH = maxY - minY;

        Vector3 pos = cam.transform.position;

        pos.x = (gridW <= viewW) ? (minX + maxX) * 0.5f : Mathf.Clamp(pos.x, minX + halfW, maxX - halfW);
        pos.y = (gridH <= viewH) ? (minY + maxY) * 0.5f : Mathf.Clamp(pos.y, minY + halfH, maxY - halfH);

        cam.transform.position = pos;
    }

    private bool IsPointerOverUI(int fingerId = -1)
    {
        if (!ignoreDragOverUI) return false;
        if (EventSystem.current == null) return false;

        if (fingerId == -1) return EventSystem.current.IsPointerOverGameObject();
        return EventSystem.current.IsPointerOverGameObject(fingerId);
    }
}
