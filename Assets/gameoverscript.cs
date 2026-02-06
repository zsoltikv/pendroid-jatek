using UnityEngine;
using TMPro;

public class gameoverscript : MonoBehaviour
{
    public TMP_Text resultText;
    void Start()
    {
        if (ResourceManager.instance.hasWon)
        {
            resultText.text = "Congratulations! You won!";
        }
        else
        {
            resultText.text = "Game Over! You lost!";
        }
    }

}
