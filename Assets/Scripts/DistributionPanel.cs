using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DistributionPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public TextMeshProUGUI buildingNameText;
    public TextMeshProUGUI internalCapacityText;
    public Transform destinationSlotsContainer;
    public GameObject destinationSlotPrefab;

    public GameObject producerRoot;
    public GameObject storageRoot;

    public TextMeshProUGUI woodStorageText;
    public TextMeshProUGUI stoneStorageText;
    public TextMeshProUGUI electricityStorageText;
    public TextMeshProUGUI waterStorageText;

    [Header("Current Building")]
    private Building currentBuilding;
    private List<DestinationSlot> destinationSlots = new List<DestinationSlot>();

    public static DistributionPanel Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        panel.SetActive(false);
    }

    void FixedUpdate()
    {
        if (currentBuilding != null)
        {
            if (currentBuilding.data.buildingType == BuildingType.Producer)
            {
                switch (currentBuilding.data.productionType)
                {
                    case ProductionType.None:
                        break;
                    case ProductionType.Water:
                        internalCapacityText.text = $"Capacity: {currentBuilding.internalWaterStorage} / {currentBuilding.maxInternalStorageCapacity}";
                        break;
                    case ProductionType.Wood:
                        internalCapacityText.text = $"Capacity: {currentBuilding.currentWoodStorage} / {currentBuilding.maxStorageCapacity}";
                        break;
                    case ProductionType.Stone:
                        internalCapacityText.text = $"Capacity: {currentBuilding.currentStoneStorage} / {currentBuilding.maxStorageCapacity}";
                        break;
                    case ProductionType.Electricity:
                        internalCapacityText.text = $"Capacity: {currentBuilding.internalElectricityStorage} / {currentBuilding.maxInternalStorageCapacity}";
                        break;
                    case ProductionType.Food:
                        internalCapacityText.text = $"Capacity: {currentBuilding.internalFoodStorage} / {currentBuilding.maxStorageCapacity}";
                        break;
                    default:
                        break;
                }
            }
            else if (currentBuilding.data.buildingType == BuildingType.Storage)
            {
                woodStorageText.text = $"Wood: {currentBuilding.currentWoodStorage}";
                stoneStorageText.text = $"Stone: {currentBuilding.currentStoneStorage}";
                waterStorageText.text = $"Water: {currentBuilding.internalWaterStorage}";
                waterStorageText.text = $"Water: {currentBuilding.internalElectricityStorage}";
            }
        }
    }

    public void OpenPanel(Building building)
    {
        if (building == null || !building.isConstructed) return;

        currentBuilding = building;
        panel.SetActive(true);

        buildingNameText.text = building.data.buildingName;

        if (currentBuilding.data.buildingType == BuildingType.Producer)
            CreateDestinationSlots();

        storageRoot.SetActive(currentBuilding.data.buildingType == BuildingType.Storage);
        producerRoot.SetActive(currentBuilding.data.buildingType == BuildingType.Producer);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        currentBuilding = null;
        ClearDestinationSlots();
    }

    private void CreateDestinationSlots()
    {
        ClearDestinationSlots();

        int maxSlots = currentBuilding.GetMaxDestinations();

        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObj = Instantiate(destinationSlotPrefab, destinationSlotsContainer);
            DestinationSlot slot = slotObj.GetComponent<DestinationSlot>();

            slot.Initialize(this, i);

            if (i < currentBuilding.destinations.Count)
            {
                slot.SetDestination(currentBuilding.destinations[i]);
            }

            destinationSlots.Add(slot);
        }
    }

    private void ClearDestinationSlots()
    {
        foreach (var slot in destinationSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        destinationSlots.Clear();
    }

    public List<Building> GetAvailableBuildings()
    {
        Building[] allBuildings = GameObject.FindObjectsByType<Building>(FindObjectsSortMode.None);

        List<Building> availableBuildings = new List<Building>();

        foreach (var building in allBuildings)
        {
            if (building == currentBuilding) continue;
            if (!building.isConstructed) continue;

            if (CanAcceptResource(building, currentBuilding.data.productionType))
            {
                availableBuildings.Add(building);
            }
        }

        return availableBuildings;
    }

    public List<Building> GetAvailableBuildingsForSlot(int excludeSlotIndex)
    {
        List<Building> allAvailable = GetAvailableBuildings();
        List<Building> filtered = new List<Building>();

        foreach (var building in allAvailable)
        {
            bool alreadyUsed = false;

            for (int i = 0; i < currentBuilding.destinations.Count; i++)
            {
                if (i == excludeSlotIndex) continue;

                if (currentBuilding.destinations[i] == building)
                {
                    alreadyUsed = true;
                    break;
                }
            }

            if (!alreadyUsed)
            {
                filtered.Add(building);
            }
        }

        return filtered;
    }

    private bool CanAcceptResource(Building building, ProductionType resourceType)
    {
        if (building.data.buildingType == BuildingType.Storage)
            return true;

        switch (resourceType)
        {
            case ProductionType.Water:
                return building.data.requiresWater;
            case ProductionType.Electricity:
                return building.data.requiresElectricity;
            case ProductionType.Wood:
                return building.data.requiresWood;
            case ProductionType.Stone:
                return building.data.requiresStone;
            default:
                return false;
        }
    }

    public void UpdateDestination(int slotIndex, Building newDestination)
    {
        if (slotIndex < currentBuilding.destinations.Count)
        {
            currentBuilding.destinations[slotIndex] = null;
        }

        while (currentBuilding.destinations.Count <= slotIndex)
        {
            currentBuilding.destinations.Add(null);
        }

        currentBuilding.destinations[slotIndex] = newDestination;

        CleanupDestinations();
    }

    public void RefreshAllSlotsExcept(int excludeSlotIndex)
    {
        foreach (var slot in destinationSlots)
        {
            if (slot != null && slot.GetSlotIndex() != excludeSlotIndex)
            {
                slot.RefreshDropdown();
            }
        }
    }

    private void CleanupDestinations()
    {
        for (int i = currentBuilding.destinations.Count - 1; i >= 0; i--)
        {
            if (currentBuilding.destinations[i] == null)
                currentBuilding.destinations.RemoveAt(i);
            else
                break;
        }
    }
}