using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingData data;
    
    [Header("Current State")]
    public bool isConstructed = false;
    public float currentWaterStorage = 0f;
    
    private SpriteRenderer spriteRenderer;
    private float productionTimer = 0f;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Update()
    {
        if (!isConstructed) return;
        

        if (data.producesWater && WeatherController.instance.IsRaining())
        {
            productionTimer += Time.deltaTime;
            
            if (productionTimer >= 1f)
            {
                float waterToCollect = data.waterProductionRate * productionTimer;
                float actualCollected = Mathf.Min(waterToCollect, data.waterStorageCapacity - currentWaterStorage);
                
                currentWaterStorage += actualCollected;
                currentWaterStorage = Mathf.Min(currentWaterStorage, data.waterStorageCapacity);
                
                productionTimer = 0f;
            }
        }
        
        // Vízfogyasztás
        if (data.requiresWater)
        {
            float consumption = data.waterConsumptionRate * Time.deltaTime;
            if (ResourceManager.instance != null)
            {
                ResourceManager.instance.ConsumeWater(consumption);
            }
        }
    }
    
    public void Construct()
    {
        isConstructed = true;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
        
        Debug.Log($"{data.buildingName} constructed at {transform.position}");
    }
    
    public float CollectStoredWater()
    {
        float collected = currentWaterStorage;
        currentWaterStorage = 0f;
        return collected;
    }
    
    public float GetStoragePercentage()
    {
        if (data.waterStorageCapacity <= 0) return 0f;
        return currentWaterStorage / data.waterStorageCapacity;
    }
}
