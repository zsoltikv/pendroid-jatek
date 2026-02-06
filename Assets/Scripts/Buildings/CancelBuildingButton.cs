using UnityEngine;
using UnityEngine.UI;

public class CancelBuildingButton : MonoBehaviour
{
    public Button cancelButton;

    private void Start()
    {
        if (cancelButton == null)
            cancelButton = GetComponent<Button>();

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelConstruction);
        }
    }

    private void Update()
    {
        // Show/hide button based on whether we're placing a building
        if (cancelButton != null && BuildingManager.instance != null)
        {
            cancelButton.gameObject.SetActive(BuildingManager.instance.isPlacingBuilding);
        }
    }

    private void CancelConstruction()
    {
        if (BuildingManager.instance != null)
        {
            BuildingManager.instance.CancelPlacement();
            Debug.Log("Building construction cancelled");
        }
    }
}
