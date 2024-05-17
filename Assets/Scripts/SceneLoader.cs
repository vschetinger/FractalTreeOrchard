using UnityEngine;
using UnityEngine.SceneManagement;  // Include this to use the scene management functions

public class SceneLoader : MonoBehaviour
{
    public string sceneToLoad;  // Public variable to set the scene name in the inspector

    // This function is called when another object enters the trigger collider
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Check if the object is tagged as "Player"
        {
            SceneManager.LoadScene(sceneToLoad);  // Load the scene
        }
    }
}
