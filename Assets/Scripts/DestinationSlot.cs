using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DestinationSlot : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown dropdown;
    public TextMeshProUGUI slotNumberText;

    private DistributionPanel panel;
    private int slotIndex;
    private Building selectedBuilding;
    private List<Building> availableBuildings;

    private bool isUpdating = false;

    public void Initialize(DistributionPanel panel, int index)
    {
        this.panel = panel;
        this.slotIndex = index;

        slotNumberText.text = $"Destination {index + 1}";

        PopulateDropdown();

        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void PopulateDropdown()
    {
        dropdown.ClearOptions();

        availableBuildings = panel.GetAvailableBuildingsForSlot(slotIndex);

        List<string> options = new List<string>();
        options.Add("None");

        foreach (var building in availableBuildings)
        {
            string buildingInfo = $"{building.data.buildingName} ({building.transform.position.x:F0}, {building.transform.position.y:F0})";
            options.Add(buildingInfo);
        }

        dropdown.AddOptions(options);
        dropdown.value = 0;
    }

    public void SetDestination(Building building)
    {
        isUpdating = true;

        if (building == null)
        {
            dropdown.value = 0;
            selectedBuilding = null;
        }
        else
        {
            int index = availableBuildings.IndexOf(building);
            if (index >= 0)
            {
                dropdown.value = index + 1;
                selectedBuilding = building;
            }
            else
            {
                dropdown.value = 0;
                selectedBuilding = null;
            }
        }

        isUpdating = false;
    }

    private void OnDropdownValueChanged(int value)
    {
        if (isUpdating) return;

        if (value == 0)
        {
            selectedBuilding = null;
            panel.UpdateDestination(slotIndex, null);
        }
        else
        {
            selectedBuilding = availableBuildings[value - 1];
            panel.UpdateDestination(slotIndex, selectedBuilding);
        }

        panel.RefreshAllSlotsExcept(slotIndex);
    }

    public void RefreshDropdown()
    {
        Building previousSelection = selectedBuilding;

        dropdown.onValueChanged.RemoveAllListeners();

        PopulateDropdown();
        SetDestination(previousSelection);

        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    public int GetSlotIndex()
    {
        return slotIndex;
    }
}