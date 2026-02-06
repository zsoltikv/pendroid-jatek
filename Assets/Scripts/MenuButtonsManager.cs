using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonsManager : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("MainScene");
    }
}