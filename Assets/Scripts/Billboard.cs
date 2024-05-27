using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        Vector3 targetPosition = mainCamera.transform.position;
        targetPosition.y = transform.position.y; // Keep the text upright
        transform.LookAt(targetPosition);
    }
}