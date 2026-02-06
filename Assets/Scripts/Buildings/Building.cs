using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingData data;

    [Header("Current State")]
    public bool isConstructed = false;
    public bool isOperational = true;

    [Header("Storage")]
    public float maxStorageCapacity;
    public float currentWoodStorage = 0f;
    public float currentStoneStorage = 0f;
    public float currentFoodStorage = 0f;
    
    [Header("Internal Supply Storage")]
    public float maxInternalStorageCapacity = 0f;
    public float internalWaterStorage = 0f;
    public float internalElectricityStorage = 0f;
    public float internalFoodStorage = 0f;
    
    [Header("Destinations")]
    public List<Building> destinations = new List<Building>();

    private SpriteRenderer spriteRenderer;
    private float productionTimer = 0f;
    private int maxDestinations;
    private int tickCounter = 0;

    public DistributionPanel distributionPanel;

    private void OnMouseDown()
    {
        if (!isConstructed) return;

        if (distributionPanel == null)
        {
            distributionPanel = GameObject.FindObjectsByType<DistributionPanel>(FindObjectsSortMode.None).FirstOrDefault();
        }

        if (distributionPanel != null && data.buildingType != BuildingType.None)
        {
            distributionPanel.OpenPanel(this);
        }
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (data != null)
        {
            maxDestinations = data.buildingType == BuildingType.Storage ? 4 : 2;
        }
    }

    private void Start()
    {
        maxStorageCapacity = data.maxStorageCapacity;
        maxInternalStorageCapacity = data.internalStorage;
    }
    
    private void Update()
    {
        if (!isConstructed) return;
        
        if (!HasEnoughInternalResources() || IsStorageFull())
        {
            isOperational = false;
            return;
        }
        
        isOperational = true;
        
        productionTimer += Time.deltaTime;
        if (productionTimer >= data.productionInterval)
        {
            tickCounter++;
            
            ConsumeInternalResources();

            if (data.buildingType == BuildingType.Producer)
            {
                ProduceResources();
            }
            
            // Házak ételfogyasztása minden 5. tickben
            if (data.buildingType == BuildingType.Population && tickCounter >= 5)
            {
                ConsumeFoodForPopulation();
                tickCounter = 0;
            }

            DistributeResources();
            
            productionTimer = 0f;
        }
    }

    private void ProduceResources()
    {
        // Produce to internal storage
        switch (data.productionType)
        {
            case ProductionType.Water:
                if (internalWaterStorage >= data.internalStorage) return;
                if (WeatherController.instance.currentWeather == WeatherController.WeatherType.Rain)
                {
                    internalWaterStorage = Mathf.Min(internalWaterStorage + data.productionRate * 1f, data.internalStorage);
                }
                if (WeatherController.instance.currentWeather == WeatherController.WeatherType.Storm)
                {
                    internalWaterStorage = Mathf.Min(internalWaterStorage + data.productionRate * 1.25f, data.internalStorage);
                }
                if (WeatherController.instance.currentWeather == WeatherController.WeatherType.Dry)
                {
                    internalWaterStorage = Mathf.Min(internalWaterStorage + data.productionRate * 0.15f, data.internalStorage);
                }
                break;
            case ProductionType.Wood:
                if (currentWoodStorage >= maxStorageCapacity) return;
                currentWoodStorage = Mathf.Min(currentWoodStorage + data.productionRate, maxStorageCapacity);
                break;
            case ProductionType.Stone:
                if (currentStoneStorage >= maxStorageCapacity) return;
                currentStoneStorage = Mathf.Min(currentStoneStorage + data.productionRate, maxStorageCapacity);
                break;
            case ProductionType.Electricity:
                if (internalElectricityStorage >= data.internalStorage) return;
                if (WeatherController.instance.currentWeather == WeatherController.WeatherType.Storm)
                {
                    internalElectricityStorage = Mathf.Min(internalElectricityStorage + data.productionRate * 0.3f, data.internalStorage);
                }
                if(WeatherController.instance.currentWeather == WeatherController.WeatherType.Rain)
                {
                    internalElectricityStorage = Mathf.Min(internalElectricityStorage + data.productionRate * 0.6f, data.internalStorage);
                }
                if (WeatherController.instance.currentWeather == WeatherController.WeatherType.Dry)
                {
                    internalElectricityStorage = Mathf.Min(internalElectricityStorage + data.productionRate * 1, data.internalStorage);
                }
                break;
            case ProductionType.Food:
                if (internalFoodStorage >= data.internalStorage) return;
                internalFoodStorage = Mathf.Min(internalFoodStorage + data.productionRate, data.internalStorage);
                break;
        }
    }
    
    private void DistributeResources()
    {
        if (destinations.Count == 0) return;
        
        float totalToDistribute = GetCurrentStorage();
        if (totalToDistribute <= 0) return;
        
        float amountPerDestination = totalToDistribute / destinations.Count;
        
        foreach (var destination in destinations)
        {
            if (destination == null || !destination.isConstructed) continue;
            
            float amountToSend = Mathf.Min(amountPerDestination, destination.GetAvailableCapacity(data.productionType));
            if (amountToSend > 0)
            {
                TransferResource(destination, amountToSend);
            }
        }
    }
    
    private void TransferResource(Building destination, float amount)
    {
        switch (data.productionType)
        {
            case ProductionType.Water:
                internalWaterStorage -= amount;
                float waterCapacity = destination.data.requiresWater ? destination.maxInternalStorageCapacity : destination.maxStorageCapacity;
                float waterSpace = waterCapacity - destination.internalWaterStorage;
                float waterToSend = Mathf.Min(amount, waterSpace);
                destination.internalWaterStorage += waterToSend;
                break;
            case ProductionType.Wood:
                currentWoodStorage -= amount;
                destination.currentWoodStorage += amount;
                break;
            case ProductionType.Stone:
                currentStoneStorage -= amount;
                destination.currentStoneStorage += amount;
                break;
            case ProductionType.Electricity:
                internalElectricityStorage -= amount;
                float elecCapacity = destination.data.requiresElectricity ? destination.maxInternalStorageCapacity : destination.maxStorageCapacity;
                float elecSpace = elecCapacity - destination.internalElectricityStorage;
                float elecToSend = Mathf.Min(amount, elecSpace);
                destination.internalElectricityStorage += elecToSend;
                break;
            case ProductionType.Food:
                internalFoodStorage -= amount;
                float foodCapacity = destination.data.requiresFood ? destination.maxInternalStorageCapacity : destination.maxStorageCapacity;
                float foodSpace = foodCapacity - destination.internalFoodStorage;
                float foodToSend = Mathf.Min(amount, foodSpace);
                destination.internalFoodStorage += foodToSend;
                break;
        }
    }
    
    private float GetCurrentStorage()
    {
        switch (data.productionType)
        {
            case ProductionType.Water: return internalWaterStorage;
            case ProductionType.Wood: return currentWoodStorage;
            case ProductionType.Stone: return currentStoneStorage;
            case ProductionType.Electricity: return internalElectricityStorage;
            case ProductionType.Food: return internalFoodStorage;
            default: return 0f;
        }
    }
    
    private float GetAvailableCapacity(ProductionType type)
    {
        float current = 0f;
        float maxCapacity = 0f;
        
        switch (type)
        {
            case ProductionType.Water:
                current = internalWaterStorage;
                maxCapacity = data.requiresWater ? maxInternalStorageCapacity : maxStorageCapacity;
                break;
            case ProductionType.Wood:
                current = currentWoodStorage;
                maxCapacity = maxStorageCapacity;
                break;
            case ProductionType.Stone:
                current = currentStoneStorage;
                maxCapacity = maxStorageCapacity;
                break;
            case ProductionType.Electricity:
                current = internalElectricityStorage;
                maxCapacity = data.requiresElectricity ? maxInternalStorageCapacity : maxStorageCapacity;
                break;
            case ProductionType.Food:
                current = internalFoodStorage;
                maxCapacity = data.requiresFood ? maxInternalStorageCapacity : maxStorageCapacity;
                break;
        }
        return Mathf.Max(0, maxCapacity - current);
    }
    
    private bool HasEnoughInternalResources()
    {
        if (data.requiresWater && internalWaterStorage < data.waterConsumptionRate)
            return false;
        
        if (data.requiresElectricity && internalElectricityStorage < data.electricityConsumptionRate)
            return false;
        
        if (data.requiresFood && internalFoodStorage < data.foodConsumptionRate)
            return false;
        
        return true;
    }

    private bool IsStorageFull()
    {
        switch (data.productionType)
        {
            case ProductionType.Water:
                return internalWaterStorage >= maxInternalStorageCapacity;
            case ProductionType.Wood:
                return currentWoodStorage >= maxStorageCapacity;
            case ProductionType.Stone:
                return currentStoneStorage >= maxStorageCapacity;
            case ProductionType.Electricity:
                return internalElectricityStorage >= maxInternalStorageCapacity;
            case ProductionType.Food:
                return internalFoodStorage >= maxInternalStorageCapacity;
            default:
                return true;
        }
    }
    
    private void ConsumeInternalResources()
    {
        if (data.requiresWater)
        {
            internalWaterStorage -= data.waterConsumptionRate;
            internalWaterStorage = Mathf.Max(0, internalWaterStorage);
        }
        
        if (data.requiresElectricity)
        {
            internalElectricityStorage -= data.electricityConsumptionRate;
            internalElectricityStorage = Mathf.Max(0, internalElectricityStorage);
        }
        
        if (data.requiresFood)
        {
            internalFoodStorage -= data.foodConsumptionRate;
            internalFoodStorage = Mathf.Max(0, internalFoodStorage);
        }
    }
    
    private void ConsumeFoodForPopulation()
    {
        if (data.requiresFood && internalFoodStorage >= data.foodConsumptionRate)
        {
            // Van elég étel, fogyasztás
            internalFoodStorage -= data.foodConsumptionRate;
            Debug.Log($"{data.buildingName} consumed food. Remaining: {internalFoodStorage}");
        }
        else if (data.requiresFood)
        {
            // Nincs elég étel, népesség csökkenés
            if (ResourceManager.instance != null)
            {
                ResourceManager.instance.DecreasePopulation(1);
                Debug.LogWarning($"{data.buildingName} has no food! Population decreased to {ResourceManager.instance.currentPopulation}");
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
    
    
    public float GetStoragePercentage()
    {
        if (data.internalStorage <= 0) return 0f;
        return GetCurrentStorage() / data.internalStorage;
    }
    
    public bool AddDestination(Building destination)
    {
        if (destination == null || destinations.Contains(destination)) return false;
        if (destinations.Count >= maxDestinations) return false;
        
        destinations.Add(destination);
        return true;
    }
    
    public void RemoveDestination(Building destination)
    {
        destinations.Remove(destination);
    }
    
    public int GetMaxDestinations()
    {
        return maxDestinations;
    }
}
