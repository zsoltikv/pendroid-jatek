using UnityEngine;
using System;

public class WeatherController : MonoBehaviour
{
    public static WeatherController instance;
    
    [Header("Weather Settings")]
    public WeatherType currentWeather = WeatherType.Dry;
    
    [Header("Rain Settings")]
    [Tooltip("Minimum duration of rain in seconds")]
    public float minRainDuration = 30f;
    [Tooltip("Maximum duration of rain in seconds")]
    public float maxRainDuration = 120f;
    [Tooltip("Water collection rate per second during rain")]
    public float rainWaterRate = 2f;
    
    [Header("Storm Settings")]
    [Tooltip("Minimum duration of storm in seconds")]
    public float minStormDuration = 15f;
    [Tooltip("Maximum duration of storm in seconds")]
    public float maxStormDuration = 60f;
    [Tooltip("Water collection rate per second during storm")]
    public float stormWaterRate = 5f;
    
    [Header("Dry Weather Settings")]
    [Tooltip("Minimum duration of dry weather in seconds")]
    public float minDryDuration = 60f;
    [Tooltip("Maximum duration of dry weather in seconds")]
    public float maxDryDuration = 180f;
    
    [Header("Weather Probability")]
    [Range(0f, 1f)]
    [Tooltip("Chance of rain after dry period (0-1)")]
    public float rainProbability = 0.6f;
    [Range(0f, 1f)]
    [Tooltip("Chance of storm after dry period (0-1)")]
    public float stormProbability = 0.2f;
    
    [Header("Current Status")]
    [SerializeField] private float timeUntilWeatherChange;
    [SerializeField] private float currentWaterCollectionRate;
    
    // Events
    public event Action<WeatherType> OnWeatherChanged;
    public event Action<float> OnWaterCollected;
    
    public enum WeatherType
    {
        Dry,
        Rain,
        Storm
    }

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
        
        ChangeWeather(WeatherType.Dry);
    }

    private void Update()
    {
        timeUntilWeatherChange -= Time.deltaTime;
        
        if (timeUntilWeatherChange <= 0)
        {
            ChangeToNextWeather();
        }
        
    }

    private void ChangeToNextWeather()
    {
        WeatherType nextWeather = WeatherType.Dry;
        
        if (currentWeather == WeatherType.Dry)
        {
            // After dry period, decide between rain, storm, or more dry
            float roll = UnityEngine.Random.Range(0f, 1f);
            
            if (roll < stormProbability)
            {
                nextWeather = WeatherType.Storm;
            }
            else if (roll < stormProbability + rainProbability)
            {
                nextWeather = WeatherType.Rain;
            }
            else
            {
                nextWeather = WeatherType.Dry;
            }
        }
        else
        {
            nextWeather = WeatherType.Dry;
        }
        
        ChangeWeather(nextWeather);
    }

    public void ChangeWeather(WeatherType newWeather)
    {
        currentWeather = newWeather;
        
        switch (currentWeather)
        {
            case WeatherType.Dry:
                timeUntilWeatherChange = UnityEngine.Random.Range(minDryDuration, maxDryDuration);
                currentWaterCollectionRate = 0f;
                break;
                
            case WeatherType.Rain:
                timeUntilWeatherChange = UnityEngine.Random.Range(minRainDuration, maxRainDuration);
                currentWaterCollectionRate = rainWaterRate;
                break;
                
            case WeatherType.Storm:
                timeUntilWeatherChange = UnityEngine.Random.Range(minStormDuration, maxStormDuration);
                currentWaterCollectionRate = stormWaterRate;
                break;
        }
        
        Debug.Log($"Weather changed to {currentWeather}. Duration: {timeUntilWeatherChange}s, Water rate: {currentWaterCollectionRate}/s");
        
        OnWeatherChanged?.Invoke(currentWeather);
    }


    public float GetCurrentWaterCollectionRate()
    {
        return currentWaterCollectionRate;
    }
    
    public float GetTimeUntilWeatherChange()
    {
        return timeUntilWeatherChange;
    }
    
    public bool IsRaining()
    {
        return currentWeather == WeatherType.Rain || currentWeather == WeatherType.Storm;
    }
    
    public void ForceRain()
    {
        ChangeWeather(WeatherType.Rain);
    }
    
    public void ForceStorm()
    {
        ChangeWeather(WeatherType.Storm);
    }
    
    public void ForceDry()
    {
        ChangeWeather(WeatherType.Dry);
    }
}
