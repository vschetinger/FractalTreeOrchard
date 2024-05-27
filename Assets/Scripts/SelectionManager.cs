using UnityEngine;
using UnityEngine.UI;
using TMPro; // Add this namespace

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    [System.Serializable]
    public struct StartSelection
    {
        public Vector2 position;
        public string keyword;

        public StartSelection(Vector2 position, string keyword)
        {
            this.position = position;
            this.keyword = keyword;
        }
    }

    public Slider sliderX;
    public Slider sliderY;
    public TMP_InputField inputFieldBeeWord; // Change to TMP_InputField

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public StartSelection GetStartSelection()
    {
        // Get values from sliders and input field
        float xValue = sliderX.value;
        float yValue = sliderY.value;
        string beeWord = inputFieldBeeWord.text; // This will now work with TMP_InputField

        // Create and return a StartSelection struct
        return new StartSelection(new Vector2(xValue, yValue), beeWord);
    }
}