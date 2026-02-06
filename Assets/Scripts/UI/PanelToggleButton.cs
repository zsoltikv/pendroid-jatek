using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

public class PanelToggleButton : MonoBehaviour
{
    [Header("Panel object (UI)")]
    public GameObject panel;
    public GameObject container;
    public GameObject BuildOption;
    public BuildingData[] buildingOptions; 

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
    
    private void Start()
    {
        // Betöltjük az összes BuildingData asset-et
        
        // Ha nincs container vagy BuildOption prefab, nem csinálunk semmit
        if (container == null || BuildOption == null)
        {
            Debug.LogWarning("Container vagy BuildOption prefab nincs beállítva!");
            return;
        }
        
        // Minden BuildingData-hoz létrehozunk egy BuildOption példányt
        foreach (BuildingData buildingData in buildingOptions)
        {
            GameObject option = Instantiate(BuildOption, container.transform);
            
            // Lokális változó a closure probléma elkerüléséhez
            BuildingData currentBuilding = buildingData;
            
            // Itt beállíthatod a BuildOption komponenst, ha van rajta
            var button = option.GetComponentInChildren<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => {
                    BuildingManager.instance.SelectBuilding(currentBuilding);
                    Hide();
                });
            }


            option.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = currentBuilding.buildingName; // Feltételezve, hogy a név a gyerek Text komponensben van
            if (currentBuilding.woodCost > 0)
                option.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text += $"Wood: {currentBuilding.woodCost}\n";
            if (currentBuilding.stoneCost > 0)
                option.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text += $"Stone: {currentBuilding.stoneCost}";
            option.SetActive(true);
        }
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