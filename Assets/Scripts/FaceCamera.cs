using UnityEngine;

public class FaceCameraAndRenderOnTop : MonoBehaviour
{
    private Camera mainCamera;
    private Canvas canvas;

    void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("No Canvas component found on the GameObject.");
            canvas = gameObject.AddComponent<Canvas>();
        }

        // Set the canvas to World Space
        canvas.renderMode = RenderMode.WorldSpace;

        // Set sorting layer and order
        canvas.sortingLayerName = "UIOnTop";
        canvas.sortingOrder = 999; // High value to ensure it renders on top

        // Optional: Add a CanvasRenderer if it's not already there
        if (GetComponent<CanvasRenderer>() == null)
        {
            gameObject.AddComponent<CanvasRenderer>();
        }
    }

    void Update()
    {
        if (mainCamera != null)
        {
            // Make the text face the camera
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
    }
}