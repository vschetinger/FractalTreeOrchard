using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameSceneName = "GameScene";

    private void Start()
    {
        // Disable the Play button initially

        // Subscribe to the embeddingsLoaded event in the GameManager
    }

    private void OnDestroy()
    {
        // Unsubscribe from the embeddingsLoaded event when the MainMenu is destroyed
    }


    public void StartGame()
    {
        // Load the game scene when the Play button is clicked
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}