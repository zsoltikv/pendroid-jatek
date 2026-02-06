using UnityEngine;

public enum BuildingType { None, Producer, Storage }
public enum ProductionType { None, Water, Wood, Stone, Electricity }

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

    [Header("Costs")]
    public int woodCost;
    public int stoneCost;

    [Header("Production")]
    public BuildingType buildingType = BuildingType.None;
    public ProductionType productionType;
    public float productionRate = 1f; // egység/tick
    public float productionInterval = 1f; // másodperc (1 tick = 1 mp)
    public float internalStorage; // maximum mennyiség, amit a gyár tárolhat a termeléshez
    public float maxStorageCapacity; // maximum mennyiség, amit a raktár tárolhat


    [Header("Requirements")]
    public bool requiresWater;
    public float waterConsumptionRate;
    public bool requiresElectricity;
    public float electricityConsumptionRate; // áram/másodperc fogyasztás
    public float requiredWaterStorage;
    public float requiredElectricityStorage;
    
    [Header("Visual")]
    public Color previewColor = new Color(0, 1, 0, 0.5f);
    public Color invalidPlacementColor = new Color(1, 0, 0, 0.5f);
}
