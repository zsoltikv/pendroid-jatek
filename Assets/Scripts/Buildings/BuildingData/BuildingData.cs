using UnityEngine;

[CreateAssetMenu(fileName = "New Building", menuName = "City Builder/Building Data")]
public class BuildingData : ScriptableObject
{
    [Header("Basic Info")]
    public string buildingName;
    [TextArea(2, 4)] public string description;
    public Sprite icon;
    public GameObject prefab;

    [Header("Grid Size")]
    public int width = 1;
    public int height = 1;

    [Header("Role & Resource")]
    public BuildingRole role;
    public ResourceType resourceType; // víz/fa/kő/áram - amit kezel

    [Header("Costs")]
    public int waterCost;
    public int woodCost;
    public int stoneCost;

    [Header("Production")]
    public bool producesWater;
    public float waterProductionRate;
    public float waterStorageCapacity;

    public bool producesWood;
    public float woodProductionRate;

    public bool producesStone;
    public float stoneProductionRate;

    public bool producesElectricity;
    public float electricityProductionRate;

    [Header("Requirements")]
    public bool requiresWater;
    public float waterConsumptionRate;
    public bool requiresElectricity;
    public float electricityConsumptionRate;

    [Header("Visual")]
    public Color previewColor = new Color(0, 1, 0, 0.5f);
    public Color invalidPlacementColor = new Color(1, 0, 0, 0.5f);
}
