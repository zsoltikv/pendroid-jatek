using UnityEngine;

[CreateAssetMenu(fileName = "New Building", menuName = "City Builder/Building Data")]
public class BuildingData : ScriptableObject
{
    [Header("Basic Info")]
    public string buildingName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;
    public GameObject prefab;
    
    [Header("Grid Size")]
    public int width = 1;
    public int height = 1;
    
    [Header("Costs")]
    public int waterCost;
    public int woodCost;
    public int stoneCost;
    
    [Header("Production")]
    public bool producesWater;
    public float waterProductionRate; // víz/másodperc (csak esőben)
    public float waterStorageCapacity;
    
    [Header("Requirements")]
    public bool requiresWater; // Szükséges-e víz a működéshez
    public float waterConsumptionRate; // víz/másodperc fogyasztás
    
    [Header("Visual")]
    public Color previewColor = new Color(0, 1, 0, 0.5f); // Zöld átlátszó alapértelmezetten
    public Color invalidPlacementColor = new Color(1, 0, 0, 0.5f); // Piros átlátszó
}
