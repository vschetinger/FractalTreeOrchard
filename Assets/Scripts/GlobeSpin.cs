using UnityEngine;

public class GlobeSpin : MonoBehaviour
{
    public float globeRotationSpeed = 10f;
    public Vector3 globeRotationAxis = Vector3.up;
    public float lightOrbitSpeed = 0.5f;
    public float lightOrbitRadius = 5f;
    public float lightOrbitTilt = 20f;

    private Transform directionalLight;
    private Transform cameraTransform;

    private void Start()
    {
        // Find the directional light in the scene
        directionalLight = GameObject.Find("Directional Light").transform;

        // Find the main camera in the scene
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        // Calculate the rotation axis based on camera direction
        Vector3 cameraDirection = cameraTransform.forward;
        Vector3 rotationAxis = Vector3.Lerp(globeRotationAxis, cameraDirection, 0.5f);

        // Rotate the globe around the calculated axis
        transform.Rotate(rotationAxis, globeRotationSpeed * Time.deltaTime);

        // Calculate the new position of the light based on orbit with tilt
        float angle = Time.time * lightOrbitSpeed;
        float x = Mathf.Cos(angle) * lightOrbitRadius;
        float y = Mathf.Sin(angle) * lightOrbitRadius * Mathf.Tan(Mathf.Deg2Rad * lightOrbitTilt);
        float z = Mathf.Sin(angle) * lightOrbitRadius;
        Vector3 newPosition = transform.position + new Vector3(x, y, z);

        // Update the position and rotation of the directional light
        directionalLight.position = newPosition;
        directionalLight.LookAt(transform.position);
    }
}