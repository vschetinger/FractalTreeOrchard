using UnityEngine;
using TMPro; // Use this if you're using TextMeshPro

public class OverlayText : MonoBehaviour
{
    private Transform fruitTransform;
    private Transform originalTextTransform;
    private TextMeshPro originalTextMeshPro;

    private Transform copiedTextTransform;
    private TextMeshPro copiedTextMeshPro;
    private Canvas overlayCanvas;

    void Start()
    {
        fruitTransform = transform;

        // Find the child object named "FruitText"
        Transform fruitTextTransform = fruitTransform.Find("FruitText");
        if (fruitTextTransform == null)
        {
            Debug.LogError("No child object named 'FruitText' found.");
            return;
        }

        // Get the TextMeshPro component from the FruitText child
        originalTextMeshPro = fruitTextTransform.GetComponent<TextMeshPro>();
        if (originalTextMeshPro == null)
        {
            Debug.LogError("No TextMeshPro component found on 'FruitText' child.");
            return;
        }

        originalTextTransform = originalTextMeshPro.transform;

        // Find the overlay canvas in the scene by name
        GameObject canvasObject = GameObject.Find("CanvasTextOverlay");
        if (canvasObject != null)
        {
            overlayCanvas = canvasObject.GetComponent<Canvas>();
        }

        if (overlayCanvas == null)
        {
            Debug.LogError("No CanvasTextOverlay canvas found in the scene.");
            return;
        }

        // Create a copy of the original FruitText object
        GameObject copiedTextObject = Instantiate(originalTextTransform.gameObject, overlayCanvas.transform);
        copiedTextMeshPro = copiedTextObject.GetComponent<TextMeshPro>();
        copiedTextTransform = copiedTextObject.transform;

        // Configure the copied TextMeshPro settings (if needed)
        copiedTextMeshPro.fontSize = originalTextMeshPro.fontSize;
        copiedTextMeshPro.alignment = originalTextMeshPro.alignment;
        copiedTextMeshPro.text = originalTextMeshPro.text;

        // Ensure the local scale is set to one
        copiedTextTransform.localScale = Vector3.one;
    }

    void Update()
    {
        if (fruitTransform != null && copiedTextTransform != null && overlayCanvas != null)
        {
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(fruitTransform.position);

            // Set the position of the copied text object
            copiedTextTransform.position = screenPosition;
            copiedTextTransform.localScale = Vector3.one; // Ensure the scale is set to one to avoid small sizes

            // Update the text if needed
            copiedTextMeshPro.text = originalTextMeshPro.text;

            Debug.Log($"Screen Position: {screenPosition}, World Position: {fruitTransform.position}");
        }
    }

    private void OnDestroy()
    {
        // Clean up the dynamically created text object
        if (copiedTextTransform != null)
        {
            Destroy(copiedTextTransform.gameObject);
        }
    }
}