using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovementComponent : MonoBehaviour
{

    [SerializeField] private GameObject playerObject;
    [SerializeField] private float closestDistanceToPlayer;

    private float maximumDistanceFromPlayer;
    private Vector3 previousPlayerPostion;
    // Start is called before the first frame update
    void Start()
    {
        previousPlayerPostion = playerObject.transform.position;
        maximumDistanceFromPlayer = Vector3.Distance(transform.position, previousPlayerPostion);
        maximumDistanceFromPlayer = Mathf.Abs(maximumDistanceFromPlayer);
    }

    // Update is called once per frame
    void Update()
    {
        var currentPlayerPosition = playerObject.transform.position;
        var deltaPlayerPosition = currentPlayerPosition - previousPlayerPostion;
        transform.position += deltaPlayerPosition;
        previousPlayerPostion = currentPlayerPosition;
    }

    void OnCameraMovement(InputValue inputValue)
    {
        Vector2 inputVector = inputValue.Get<Vector2>();
//        Debug.Log("Camera Movement triggered");
        var playerTransform = playerObject.GetComponent<Transform>();
        this.transform.RotateAround(playerTransform.position, new Vector3(0,1,0),  inputVector.x);
        playerTransform.rotation = Quaternion.Euler(0, this.transform.rotation.eulerAngles.y, 0);
        var shiftValue = inputVector.y * 0.03f;
        //Magic Line of Code. --> Use shiftValue to "shift" the camera closer of further away to/from the player.
        //However: This shifting should aways be related to the cameras orientation/rotation... So it should always move "forward" from the carmeras perspective
        //So, just increasting the cameras world position on the z axis is not the right way.
        transform.Translate(new Vector3(0, 0, shiftValue));

        var distance = Vector3.Distance(transform.position, playerTransform.position);
        distance = Mathf.Abs(distance);

        if (distance > maximumDistanceFromPlayer || distance < closestDistanceToPlayer)
        {
            transform.Translate(new Vector3(0, 0, -shiftValue));

            //invert the previous shift... Means: Copy/Paste the "Magic Line of code" and put a single mathematical symbol at the right place within that "Magic Line of Code"
        }
        


    }
}
