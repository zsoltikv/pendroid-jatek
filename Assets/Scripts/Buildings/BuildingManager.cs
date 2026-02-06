using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager instance;
    
    [Header("Building System")]
    public BuildingData selectedBuilding;
    public bool isTownHallPlaced = false;
    public bool isPlacingBuilding = false;
    [Header("Preview")]
    private GameObject previewObject;
    private SpriteRenderer previewRenderer;
    
    [Header("Camera")]
    public Camera mainCamera;
    
    private List<Building> allBuildings = new List<Building>();
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (mainCamera == null)
            mainCamera = Camera.main;
    }
    
    private void Update()
    {
        if (isPlacingBuilding && selectedBuilding != null)
        {
            UpdateBuildingPreview();
            
            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceBuilding();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }
        }
    }
    
    public void SelectBuilding(BuildingData building)
    {
        selectedBuilding = building;
        if (selectedBuilding.buildingName == "Town Hall" && isTownHallPlaced)
        {
            Debug.Log("Town Hall already placed!");
            selectedBuilding = null;
            return;
        }
        isPlacingBuilding = true;
        CreatePreview();
    }
    
    private void CreatePreview()
    {
        if (previewObject != null)
            Destroy(previewObject);
        
        if (selectedBuilding == null || selectedBuilding.prefab == null)
            return;
        
        previewObject = Instantiate(selectedBuilding.prefab);
        previewRenderer = previewObject.GetComponent<SpriteRenderer>();
        
        if (previewRenderer != null)
        {
            previewRenderer.color = selectedBuilding.previewColor;
            previewRenderer.sortingOrder = 1000; // Render on top
        }
        
        // Disable building component on preview
        Building buildingComp = previewObject.GetComponent<Building>();
        if (buildingComp != null)
            buildingComp.enabled = false;
    }
    
    private void UpdateBuildingPreview()
    {
        if (previewObject == null || mainCamera == null)
            return;
        
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        Vector2Int gridPos = GridSystem.instance.GetGridPosition(mousePos);
        
        float cellSize = GridSystem.instance.cellSize;
        float buildingWidth = selectedBuilding.width * cellSize;
        float buildingHeight = selectedBuilding.height * cellSize;
        
        Vector3 worldPos = GridSystem.instance.GetWorldPosition(gridPos.x, gridPos.y);
        worldPos.x += buildingWidth / 2f;
        worldPos.y += buildingHeight / 2f - cellSize;
        
        previewObject.transform.position = worldPos;
        
        bool canPlace = CanPlaceBuilding(gridPos.x, gridPos.y);
        if (previewRenderer != null)
        {
            previewRenderer.color = canPlace ? selectedBuilding.previewColor : selectedBuilding.invalidPlacementColor;
        }
    }
    
    private bool CanPlaceBuilding(int x, int y)
    {
        if (GridSystem.instance == null || selectedBuilding == null)
            return false;
        
        // griden jó-e?
        if (!GridSystem.instance.IsPositionValid(x, y, selectedBuilding.width, selectedBuilding.height))
            return false;
        
        // Cvan-e elég nyersanyag?
        if (ResourceManager.instance != null && !ResourceManager.instance.HasEnoughResources(selectedBuilding))
            return false;
        
        return true;
    }
    
    private void TryPlaceBuilding()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        Vector2Int gridPos = GridSystem.instance.GetGridPosition(mousePos);
        
        if (!CanPlaceBuilding(gridPos.x, gridPos.y))
        {
            Debug.Log("Cannot place building here!");
            return;
        }
        
        if (ResourceManager.instance != null && !ResourceManager.instance.SpendResources(selectedBuilding))
        {
            Debug.Log("Not enough resources!");
            return;
        }
        
        // Calculate the center of the building area (same as preview)
        float cellSize = GridSystem.instance.cellSize;
        float buildingWidth = selectedBuilding.width * cellSize;
        float buildingHeight = selectedBuilding.height * cellSize;
        
        Vector3 worldPos = GridSystem.instance.GetWorldPosition(gridPos.x, gridPos.y);
        worldPos.x += buildingWidth / 2f;
        worldPos.y += buildingHeight / 2f - cellSize;
        
        GameObject buildingObj = Instantiate(selectedBuilding.prefab, worldPos, Quaternion.identity);
        Building building = buildingObj.GetComponent<Building>();
        
        if (building != null)
        {
            building.data = selectedBuilding;
            building.Construct();
            allBuildings.Add(building);
        }
        
        // Update grid
        GridSystem.instance.PlaceBuilding(building, gridPos.x, gridPos.y, selectedBuilding.width, selectedBuilding.height);
        
        if (selectedBuilding.buildingName == "Town Hall")
        {
            isTownHallPlaced = true;
        }
        
        // Update max population
        if (ResourceManager.instance != null)
        {
            ResourceManager.instance.UpdateMaxPopulation();
        }

    }
    
    public void CancelPlacement()
    {
        isPlacingBuilding = false;
        selectedBuilding = null;
        
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }
    }
    
    public List<Building> GetAllBuildings()
    {
        return new List<Building>(allBuildings);
    }
    
    public void RemoveBuilding(Building building)
    {
        if (building == null) return;
        
        allBuildings.Remove(building);
        
        Vector2Int gridPos = GridSystem.instance.GetGridPosition(building.transform.position);
        GridSystem.instance.RemoveBuilding(gridPos.x, gridPos.y, building.data.width, building.data.height);
        
        Destroy(building.gameObject);
        
        // Update max population
        if (ResourceManager.instance != null)
        {
            ResourceManager.instance.UpdateMaxPopulation();
        }
    }
}
