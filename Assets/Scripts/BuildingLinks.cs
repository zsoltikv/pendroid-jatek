using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ResourceLink
{
    public Building target;   // aki kapja
    [Range(0f, 100f)]
    public float percent;     // mennyi %-ot kapjon
}

public class BuildingLinks : MonoBehaviour
{
    public Building owner;

    // STORAGE: 3 consumer slot
    public List<ResourceLink> outputs = new List<ResourceLink>(3);

    // GENERATOR: 1 storage target
    public ResourceLink generatorOutput = new ResourceLink();

    private void Awake()
    {
        if (owner == null) owner = GetComponent<Building>();

        while (outputs.Count < 3) outputs.Add(new ResourceLink());
        if (outputs.Count > 3) outputs.RemoveRange(3, outputs.Count - 3);
    }

    public void NormalizeStoragePercents()
    {
        float sum = 0f;
        for (int i = 0; i < outputs.Count; i++)
            sum += Mathf.Max(0f, outputs[i].percent);

        if (sum <= 0.0001f) return;

        for (int i = 0; i < outputs.Count; i++)
            outputs[i].percent = (Mathf.Max(0f, outputs[i].percent) / sum) * 100f;
    }
}
