using UnityEngine;
using UnityEngine.InputSystem;

public class MovementComponent : MonoBehaviour
{
    [SerializeField]
    private float speed = 0.1f;
    [SerializeField]
    private MovementType movementType;
    [SerializeField]
    private float movementEnergyConsumption = 0.1f;
    [SerializeField]
    private float jumpEnergyConsumption = 1f;
    [SerializeField]
    private float rotationSpeed = 100f;

    private Vector3 moveBy;
    private Rigidbody rb;
    private float rotationInput;

    private GameControls controls; // Update with the correct generated class name

    private void Awake()
    {
        controls = new GameControls(); // Ensure this matches the generated class name
        controls.Player.RotateCamera.performed += ctx => rotationInput = ctx.ReadValue<float>();
        controls.Player.RotateCamera.canceled += ctx => rotationInput = 0;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnMovement(InputValue input)
    {
        Vector2 inputValue = input.Get<Vector2>();
        moveBy = new Vector3(inputValue.x, 0, inputValue.y);
    }

    void OnJump(InputValue input)
    {
        if (GameManager.instance.currentEnergy >= jumpEnergyConsumption)
        {
            rb.AddForce(0, 8, 0, ForceMode.VelocityChange);
            GameManager.instance.currentEnergy -= jumpEnergyConsumption;
        }
    }

    void Update()
    {
        ExecuteMovement();
        ExecuteRotation();
    }

    void ExecuteMovement()
    {
        if (GameManager.instance.currentEnergy > 0f)
        {
            if (movementType == MovementType.TransformBased)
            {
                transform.Translate(moveBy * (speed * Time.deltaTime));
            }
            else if (movementType == MovementType.PhysicsBased)
            {
                rb.AddForce(moveBy * 2, ForceMode.Acceleration);
            }

            GameManager.instance.currentEnergy -= movementEnergyConsumption * Time.deltaTime;
        }
    }

    void ExecuteRotation()
    {
        if (rotationInput != 0)
        {
            float rotationAmount = rotationInput * rotationSpeed * Time.deltaTime;
            transform.Rotate(0, rotationAmount, 0);
            //Debug.Log("Rotating by: " + rotationAmount); // Add debug log
        }
    }
}