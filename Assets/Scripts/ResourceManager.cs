using UnityEngine;
using System;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance;
    
    [Header("Resources")]
    public float currentWater = 50f;
    public float currentWood = 100f;
    public float currentStone = 50f;
    
    [Header("Capacity")]
    public float maxWaterCapacity = 1000f;
    
    // Events
    public event Action<float> OnWaterChanged;
    public event Action<float> OnWoodChanged;
    public event Action<float> OnStoneChanged;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Subscribe to weather events
        if (WeatherController.instance != null)
        {
            WeatherController.instance.OnWaterCollected += AddWater;
        }
    }
    
    private void OnEnable()
    {
        if (WeatherController.instance != null)
        {
            WeatherController.instance.OnWaterCollected += AddWater;
        }
    }
    
    private void OnDisable()
    {
        if (WeatherController.instance != null)
        {
            WeatherController.instance.OnWaterCollected -= AddWater;
        }
    }
    
    public void AddWater(float amount)
    {
        currentWater = Mathf.Min(currentWater + amount, maxWaterCapacity);
        OnWaterChanged?.Invoke(currentWater);
    }
    
    public bool ConsumeWater(float amount)
    {
        if (currentWater >= amount)
        {
            currentWater -= amount;
            OnWaterChanged?.Invoke(currentWater);
            return true;
        }
        return false;
    }
    
    public void AddWood(float amount)
    {
        currentWood += amount;
        OnWoodChanged?.Invoke(currentWood);
    }
    
    public bool ConsumeWood(float amount)
    {
        if (currentWood >= amount)
        {
            currentWood -= amount;
            OnWoodChanged?.Invoke(currentWood);
            return true;
        }
        return false;
    }
    
    public void AddStone(float amount)
    {
        currentStone += amount;
        OnStoneChanged?.Invoke(currentStone);
    }
    
    public bool ConsumeStone(float amount)
    {
        if (currentStone >= amount)
        {
            currentStone -= amount;
            OnStoneChanged?.Invoke(currentStone);
            return true;
        }
        return false;
    }
    
    public bool HasEnoughResources(BuildingData building)
    {
        return currentWater >= building.waterCost &&
               currentWood >= building.woodCost &&
               currentStone >= building.stoneCost;
    }
    
    public bool SpendResources(BuildingData building)
    {
        if (!HasEnoughResources(building))
            return false;
        
        ConsumeWater(building.waterCost);
        ConsumeWood(building.woodCost);
        ConsumeStone(building.stoneCost);
        
        return true;
    }
}
