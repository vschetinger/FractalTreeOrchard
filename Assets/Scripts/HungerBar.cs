using UnityEngine;
using UnityEngine.UI;

public class HungerBar : MonoBehaviour
{
    public Image hungerBarImage; // Reference to the Image component for the hunger bar
    private float maxBarHeight; // The maximum height of the hunger bar when energy is full

    private void Start()
    {
        // Get the initial height of the hunger bar Image
        maxBarHeight = hungerBarImage.rectTransform.rect.width;

        // Initialize the hunger bar
        UpdateHungerBar();
    }

    private void Update()
    {
        // Update the hunger bar every frame
        UpdateHungerBar();
    }

    private void UpdateHungerBar()
    {
        // Calculate the fill percentage based on the current energy level
        float fillPercentage = GameManager.instance.currentEnergy / GameManager.instance.maxEnergy;

        // Calculate the new height of the hunger bar based on the fill percentage
        float newHeight = maxBarHeight * fillPercentage;

        // Update the size of the hunger bar Image
        hungerBarImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newHeight);
    }
}