using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingRoutingPanel : MonoBehaviour
{
    [Header("Root UI")]
    public GameObject root;

    [Header("Header")]
    public TMP_Text titleText;

    [Header("Generator View")]
    public GameObject generatorView;
    public TMP_Dropdown generatorTargetDropdown;
    public TMP_InputField generatorPercentInput; // 0-100

    [Header("Storage View (3 slots)")]
    public GameObject storageView;
    public TMP_Dropdown[] storageTargetDropdowns = new TMP_Dropdown[3];
    public TMP_InputField[] storagePercentInputs = new TMP_InputField[3];

    private Building current;
    private BuildingLinks links;

    private List<Building> cachedTargets = new List<Building>(); // dropdown mappinghoz

    private void Awake()
    {
        Hide();
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
        current = null;
        links = null;
    }

    public void ShowFor(Building building)
    {
        if (building == null) { Hide(); return; }

        current = building;
        links = building.GetComponent<BuildingLinks>();
        if (links == null) links = building.gameObject.AddComponent<BuildingLinks>();

        if (root != null) root.SetActive(true);
        if (titleText != null) titleText.text = $"{building.GetDisplayName()} – Routing";

        // role szerint nézet
        bool isGen = building.Role == BuildingRole.Generator;
        bool isStorage = building.Role == BuildingRole.Storage;

        if (generatorView != null) generatorView.SetActive(isGen);
        if (storageView != null) storageView.SetActive(isStorage);

        if (isGen) SetupGeneratorUI();
        if (isStorage) SetupStorageUI();
    }

    private void SetupGeneratorUI()
    {
        // Generator -> Storage célok listája (azonos resourceType ajánlott)
        var targets = GetBuildingsByRole(BuildingRole.Storage, current.Resource);

        PopulateDropdown(generatorTargetDropdown, targets, links.generatorOutput.target);

        // percent
        generatorPercentInput.SetTextWithoutNotify(ClampPercentToString(links.generatorOutput.percent));

        // Eventek
        generatorTargetDropdown.onValueChanged.RemoveAllListeners();
        generatorTargetDropdown.onValueChanged.AddListener(idx =>
        {
            if (idx <= 0) links.generatorOutput.target = null;
            else links.generatorOutput.target = targets[idx - 1];
        });

        generatorPercentInput.onEndEdit.RemoveAllListeners();
        generatorPercentInput.onEndEdit.AddListener(val =>
        {
            links.generatorOutput.percent = ParsePercent(val);
            generatorPercentInput.SetTextWithoutNotify(ClampPercentToString(links.generatorOutput.percent));
        });
    }

    private void SetupStorageUI()
    {
        // Storage -> Consumer célok listája (azonos resourceType ajánlott)
        var targets = GetBuildingsByRole(BuildingRole.Consumer, current.Resource);

        // biztos 3 slot
        while (links.outputs.Count < 3) links.outputs.Add(new ResourceLink());
        if (links.outputs.Count > 3) links.outputs.RemoveRange(3, links.outputs.Count - 3);

        for (int i = 0; i < 3; i++)
        {
            int slot = i;

            var dd = storageTargetDropdowns[slot];
            var input = storagePercentInputs[slot];

            if (dd == null || input == null) continue;

            PopulateDropdown(dd, targets, links.outputs[slot].target);
            input.SetTextWithoutNotify(ClampPercentToString(links.outputs[slot].percent));

            dd.onValueChanged.RemoveAllListeners();
            dd.onValueChanged.AddListener(idx =>
            {
                if (idx <= 0) links.outputs[slot].target = null;
                else links.outputs[slot].target = targets[idx - 1];
            });

            input.onEndEdit.RemoveAllListeners();
            input.onEndEdit.AddListener(val =>
            {
                links.outputs[slot].percent = ParsePercent(val);
                input.SetTextWithoutNotify(ClampPercentToString(links.outputs[slot].percent));
            });
        }
    }

    private void PopulateDropdown(TMP_Dropdown dd, List<Building> targets, Building selected)
    {
        dd.ClearOptions();
        var options = new List<string> { "-- none --" };
        int selectedIndex = 0;

        for (int i = 0; i < targets.Count; i++)
        {
            var b = targets[i];
            options.Add(b.GetDisplayName());
            if (b == selected) selectedIndex = i + 1;
        }

        dd.AddOptions(options);
        dd.SetValueWithoutNotify(selectedIndex);
        dd.RefreshShownValue();
    }

    private List<Building> GetBuildingsByRole(BuildingRole role, ResourceType resource)
    {
        // BuildingManager listájából dolgozunk
        var list = BuildingManager.instance != null ? BuildingManager.instance.GetAllBuildings() : new List<Building>();
        var result = new List<Building>();

        for (int i = 0; i < list.Count; i++)
        {
            var b = list[i];
            if (b == null) continue;
            if (b == current) continue;
            if (b.Role != role) continue;

            // csak azonos nyersanyag típushoz ajánljuk fel
            if (b.Resource != resource) continue;

            result.Add(b);
        }

        return result;
    }

    private float ParsePercent(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0f;
        s = s.Replace("%", "").Trim();
        if (!float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v))
        {
            float.TryParse(s, out v);
        }
        return Mathf.Clamp(v, 0f, 100f);
    }

    private string ClampPercentToString(float v)
    {
        v = Mathf.Clamp(v, 0f, 100f);
        return Mathf.RoundToInt(v).ToString();
    }
}
