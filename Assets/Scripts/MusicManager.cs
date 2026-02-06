using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource;

    [Header("Menu Music")]
    [SerializeField] private AudioClip menuMusic;

    [Header("Gameplay Music")]
    [SerializeField] private AudioClip gameplayMusic;

    private readonly HashSet<string> menuScenes = new HashSet<string>
    {
        "MenuScene"
    };

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (menuScenes.Contains(scene.name))
        {
            PlayMenuMusic();
        }
        else if (scene.name == "MainScene")
        {
            PlayGameplayMusic();
        }
        else
        {
            StopMusic();
        }
    }

    private void PlayMenuMusic()
    {
        if (musicSource == null || menuMusic == null) return;

        if (musicSource.clip == menuMusic && musicSource.isPlaying)
            return;

        musicSource.loop = true;
        musicSource.clip = menuMusic;
        musicSource.Play();
    }

    private void PlayGameplayMusic()
    {
        if (musicSource == null || gameplayMusic == null) return;

        if (musicSource.clip == gameplayMusic && musicSource.isPlaying)
            return;

        musicSource.loop = true; 
        musicSource.clip = gameplayMusic;
        musicSource.Play();
    }

    private void StopMusic()
    {
        if (musicSource == null) return;

        musicSource.Stop();
        musicSource.clip = null;
    }
}