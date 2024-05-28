using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TextMatrixOverlay : MonoBehaviour
{
    public TextMeshProUGUI textMatrix;

    private void Start()
    {
        if (textMatrix == null)
        {
            // Find the TextMeshProUGUI component in the scene
            textMatrix = FindObjectOfType<TextMeshProUGUI>();
        }
    }

    private void Update()
    {
        if (textMatrix != null)
        {
            int maxWordsToDisplay = 10; // Adjust this value based on the available screen space

            string collectedWordsText = "";
            int startIndex = Mathf.Max(0, GameManager.collectedWords.Count - maxWordsToDisplay);
            int endIndex = GameManager.collectedWords.Count;

            if (startIndex > 0)
            {
                collectedWordsText += "...\n";
            }

            for (int i = startIndex; i < endIndex; i++)
            {
                string word = GameManager.collectedWords[i];
                long points = GameManager.CalculatePoints(word);
                string coloredWord = $"<color={(points > 0 ? "green" : "red")}>{word} ({points})</color>";
                collectedWordsText += coloredWord + "\n";
            }

            // Display the target and avoid words in the overlay
            string targetWord = GameManager.instance.targetWord;
            string avoidWord = GameManager.instance.avoidWord;

            string currentSceneName = SceneManager.GetActiveScene().name;

            if (currentSceneName == "Scene3")
            {
                textMatrix.text = $"Score: {GameManager.score}\n\n" +
                                 $"Collected Words:\n{collectedWordsText}";
            }
            else
            {
                targetWord = GameManager.instance.targetWord;
                avoidWord = GameManager.instance.avoidWord;

                textMatrix.text = $"Score: {GameManager.score}\n\n" +
                                 $"Target Word: {targetWord}\n" +
                                 $"Avoid Word: {avoidWord}\n\n" +
                                 $"Collected Words:\n{collectedWordsText}";
            }
        }
    }
}