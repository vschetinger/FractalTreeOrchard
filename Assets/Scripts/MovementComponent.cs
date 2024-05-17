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

    private Vector3 moveBy;
    private Rigidbody rb;

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
}