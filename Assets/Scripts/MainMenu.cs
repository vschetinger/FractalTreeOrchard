using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameSceneName = "GameScene";
    public GameObject PlaySelectMenu;

    private void Start()
    {

    }

    private void OnDestroy()
    {
    }

    public void StartGame()
    {
        PlaySelectMenu.SetActive(true);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    public void StartSwarmMode()
    {
        // Get the selection from SelectionManager
        SelectionManager.StartSelection selection = SelectionManager.Instance.GetStartSelection();

        // Validate the word using GameManager
        if (GameManager.GetEmbedding(selection.keyword) != null)
        {
            // Save the struct inside GameManager
            GameManager.instance.SetStartSelection(selection);

            // Load Scene3
            SceneManager.LoadScene("Scene3");
        }
        else
        {
            Debug.LogError("Invalid word selected. Please select a valid word.");
        }
    }
}