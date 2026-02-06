using UnityEngine;
using System;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance;
    public Building townHallBuilding;

    public TMP_Text waterText;
    public TMP_Text woodText;
    public TMP_Text stoneText;

    [Header("Resources")]
    public float currentWater = 0f;
    public float currentWood = 0f;
    public float currentStone = 0f;
    public float currentElectricity = 0f;
    public float electricicy;
    
    [Header("Population")]
    public int currentPopulation = 0;
    public int maxPopulation = 0;
    
    [Header("Capacity")]
    public float maxWaterCapacity = 1000f;
    public float maxWoodCapacity = 1000f;
    public float maxStoneCapacity = 1000f;
    public float maxElectricityCapacity = 1000f;
    
    // Eventek -- ui
    public event Action<float> OnWaterChanged;
    public event Action<float> OnWoodChanged;
    public event Action<float> OnStoneChanged;
    public event Action<float> OnElectricityChanged;
    public event Action<int, int> OnPopulationChanged; // (current, max)
    
    private float populationTimer = 0f;
    private const float populationGrowthInterval = 10f;
    
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
        /*
        OnWaterChanged += UpdateWaterUI();
        OnWoodChanged += UpdateWoodUI();
        OnStoneChanged += UpdateStoneUI();
        OnElectricityChanged += UpdateElectricityUI();
        */
    }
    
    private void Update()
    {
        populationTimer += Time.deltaTime;
        
        if (populationTimer >= populationGrowthInterval)
        {
            GrowPopulation();
            populationTimer = 0f;
        }
    }

    void FixedUpdate()
    {
        if (townHallBuilding != null)
        {
            currentWater = townHallBuilding.internalWaterStorage;
            currentWood = townHallBuilding.currentWoodStorage;
            currentStone = townHallBuilding.currentStoneStorage;
        }

        waterText.text = $"Water: {currentWater}";
        woodText.text = $"Wood: {currentWood}";
        stoneText.text = $"Stone: {currentStone}";
    }

    public void AddWater(float amount)
    {
        townHallBuilding.internalWaterStorage = Mathf.Min(currentWater + amount, maxWaterCapacity);
        OnWaterChanged?.Invoke(townHallBuilding.internalWaterStorage);
    }
    
    public bool ConsumeWater(float amount)
    {
        if (townHallBuilding.internalWaterStorage >= amount)
        {
            townHallBuilding.internalWaterStorage -= amount;
            OnWaterChanged?.Invoke(townHallBuilding.internalWaterStorage);
            return true;
        }
        return false;
    }
    
    public void AddWood(float amount)
    {
        townHallBuilding.currentWoodStorage += amount;
        OnWoodChanged?.Invoke(townHallBuilding.currentWoodStorage);
    }
    
    public bool ConsumeWood(float amount)
    {
        if (townHallBuilding.currentWoodStorage >= amount)
        {
            townHallBuilding.currentWoodStorage -= amount;
            OnWoodChanged?.Invoke(townHallBuilding.currentWoodStorage);
            return true;
        }
        return false;
    }
    
    public void AddStone(float amount)
    {
        townHallBuilding.currentStoneStorage += amount;
        OnStoneChanged?.Invoke(townHallBuilding.currentStoneStorage);
    }
    
    public bool ConsumeStone(float amount)
    {
        if (townHallBuilding.currentStoneStorage >= amount)
        {
            townHallBuilding.currentStoneStorage -= amount;
            OnStoneChanged?.Invoke(townHallBuilding.currentStoneStorage);
            return true;
        }
        return false;
    }
    
    public bool HasEnoughResources(BuildingData building)
    {
        return townHallBuilding.currentWoodStorage >= building.woodCost &&
               townHallBuilding.currentStoneStorage >= building.stoneCost;
    }
    
    public bool SpendResources(BuildingData building)
    {
        if (!HasEnoughResources(building))
            return false;
        
        ConsumeWood(building.woodCost);
        ConsumeStone(building.stoneCost);
        
        return true;
    }
    
    public void UpdateMaxPopulation()
    {
        maxPopulation = 0;
        
        if (BuildingManager.instance != null)
        {
            var buildings = BuildingManager.instance.GetAllBuildings();
            foreach (var building in buildings)
            {
                if (building != null && building.isConstructed && building.data != null)
                {
                    maxPopulation += building.data.maxPopulation;
                }
            }
        }
        
        // Ha a jelenlegi populáció több mint a max, csökkentjük
        if (currentPopulation > maxPopulation)
        {
            currentPopulation = maxPopulation;
        }
        
        OnPopulationChanged?.Invoke(currentPopulation, maxPopulation);
    }
    
    private void GrowPopulation()
    {
        if (currentPopulation < maxPopulation)
        {
            currentPopulation++;
            OnPopulationChanged?.Invoke(currentPopulation, maxPopulation);
            Debug.Log($"Population grew: {currentPopulation}/{maxPopulation}");
        }
    }
    
    public void DecreasePopulation(int amount = 1)
    {
        if (currentPopulation > 0)
        {
            currentPopulation = Mathf.Max(0, currentPopulation - amount);
            OnPopulationChanged?.Invoke(currentPopulation, maxPopulation);
        }
    }

    
}
