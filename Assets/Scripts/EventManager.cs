using UnityEngine;
using System;
using System.Collections;

public class EventManager : MonoBehaviour
{
    public static EventManager instance;

    [Header("Event Timers")]
    public float minTimeBetweenEvents = 60f;
    public float maxTimeBetweenEvents = 180f;

    [Header("Epidemic")]
    public float epidemicDuration = 45f;
    public float epidemicInfectionRate = 0.2f; 

    [Header("Earthquake")]
    public float earthquakeDuration = 5f;
    public float earthquakeDamageChance = 0.3f; 

    [Header("Solar Flare")]
    public float solarFlareDuration = 30f;
    public float solarDisableChance = 0.5f; 

    public event Action<string> OnEventStarted;
    public event Action<string> OnEventEnded;

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
    }

    private void Start()
    {
        StartCoroutine(EventLoop());
    }

    private IEnumerator EventLoop()
    {
        while (true)
        {
            float wait = UnityEngine.Random.Range(minTimeBetweenEvents, maxTimeBetweenEvents);
            yield return new WaitForSeconds(wait);

            int roll = UnityEngine.Random.Range(0, 3);
            switch (roll)
            {
                case 0:
                    StartCoroutine(TriggerEpidemic());
                    break;
                case 1:
                    StartCoroutine(TriggerEarthquake());
                    break;
                case 2:
                    StartCoroutine(TriggerSolarFlare());
                    break;
            }
        }
    }

    private IEnumerator TriggerEpidemic()
    {
        Debug.Log("Epidemic started");
        OnEventStarted?.Invoke("Epidemic");

        float elapsed = 0f;
        while (elapsed < epidemicDuration)
        {
            TryAffectRandomBuilding(epidemicInfectionRate);
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        Debug.Log("Epidemic ended");
        OnEventEnded?.Invoke("Epidemic");
    }

    private IEnumerator TriggerEarthquake()
    {
        Debug.Log("Earthquake occurred");
        OnEventStarted?.Invoke("Earthquake");

        TryDamageBuildings(earthquakeDamageChance);

        yield return new WaitForSeconds(earthquakeDuration);

        Debug.Log("Earthquake ended");
        OnEventEnded?.Invoke("Earthquake");
    }

    private IEnumerator TriggerSolarFlare()
    {
        Debug.Log("Solar flare started");
        OnEventStarted?.Invoke("SolarFlare");

        TryDisableBuildings(solarDisableChance, solarFlareDuration);

        yield return new WaitForSeconds(solarFlareDuration);

        Debug.Log("Solar flare ended");
        OnEventEnded?.Invoke("SolarFlare");
    }

    private void TryAffectRandomBuilding(float chancePerSecond)
    {
        var all = BuildingManager.instance.GetAllBuildings();
        if (all == null || all.Count == 0) return;

        foreach (var b in all)
        {
            if (b == null) continue;

            if (UnityEngine.Random.value < chancePerSecond)
            {
                b.isOperational = false;
                Debug.Log($"Building {b.data.buildingName} affected by epidemic");
                StartCoroutine(RestoreBuildingAfter(b, 10f));
            }
        }
    }

    private void TryDamageBuildings(float destroyChance)
    {
        var all = BuildingManager.instance.GetAllBuildings();
        if (all == null || all.Count == 0) return;

        foreach (var b in all)
        {
            if (b == null) continue;

            if (UnityEngine.Random.value < destroyChance)
            {
                Debug.Log($"Building {b.data.buildingName} destroyed by earthquake");
                BuildingManager.instance.RemoveBuilding(b);
            }
        }
    }

    private void TryDisableBuildings(float disableChance, float duration)
    {
        var all = BuildingManager.instance.GetAllBuildings();
        if (all == null || all.Count == 0) return;

        foreach (var b in all)
        {
            if (b == null) continue;

            if (UnityEngine.Random.value < disableChance)
            {
                b.isOperational = false;
                Debug.Log($"Building {b.data.buildingName} disabled by solar flare");
                StartCoroutine(RestoreBuildingAfter(b, duration));
            }
        }
    }

    private IEnumerator RestoreBuildingAfter(Building b, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (b != null)
        {
            b.isOperational = true;
            Debug.Log($"Building {b.data.buildingName} restored after event");
        }
    }
}
