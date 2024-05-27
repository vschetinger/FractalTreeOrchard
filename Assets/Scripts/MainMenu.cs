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

    public void LoadScene(string sceneName){
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}