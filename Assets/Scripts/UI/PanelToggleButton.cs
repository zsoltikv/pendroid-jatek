using UnityEngine;

public class PanelToggleButton : MonoBehaviour
{
    [Header("Panel object (UI)")]
    public GameObject panel;

    [Header("Optional (ha nem Overlay a Canvas)")]
    public Canvas canvas; 
    public Camera uiCamera; 

    private RectTransform panelRect;
    private int shownFrame = -999;

    private void Awake()
    {
        if (panel != null)
            panelRect = panel.GetComponent<RectTransform>();

        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            uiCamera = null;
    }

    private void Update()
    {
        if (panel == null || !panel.activeSelf || panelRect == null)
            return;

        if (Time.frameCount == shownFrame)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(panelRect, Input.mousePosition, uiCamera))
                Hide();
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 pos = Input.GetTouch(0).position;
            if (!RectTransformUtility.RectangleContainsScreenPoint(panelRect, pos, uiCamera))
                Hide();
        }
    }

    public void Toggle()
    {
        if (panel == null) return;
        panel.SetActive(!panel.activeSelf);

        if (panel.activeSelf)
            shownFrame = Time.frameCount;
    }

    public void Show()
    {
        if (panel == null) return;
        panel.SetActive(true);
        shownFrame = Time.frameCount;
    }

    public void Hide()
    {
        if (panel == null) return;
        panel.SetActive(false);
    }
}