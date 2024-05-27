using UnityEngine;
using System.Collections.Generic;
public class BeeAgent : MonoBehaviour
{
    private bool embeddingsLoaded = false;

    public static List<BeeAgent> activeBeeAgents = new List<BeeAgent>();

    [Header("Movement Settings")]
    public float speed = 5f;
    public float maxJumpForce = 10f;
    public float minJumpForce = 2f;
    public float jumpCooldown = 1f;

    [Header("Energy Settings")]
    public float energy = 100f;
    public float energyConsumptionRate = 1f;

    [Header("Physics Settings")]
    public float mass = 1f;
    public float drag = 0f;
    public float angularDrag = 0.05f;

    private Transform target;
    private Rigidbody rb;
    private float lastJumpTime;
    private AgentTargetingLine targetingLine;
    private AgentPathLine pathLine;

    [Header("Bee Sense")]
    public string beeWord;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Collider col = GetComponent<Collider>();
        if (rb == null)
        {
            Debug.LogError("BeeAgent: Rigidbody component is missing!");
        }
        if (col == null)
        {
            Debug.LogError("BeeAgent: Collider component is missing!");
        }

        rb.mass = mass;
        rb.drag = drag;
        rb.angularDrag = angularDrag;

        // Freeze rotation on X and Z axes programmatically
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        FindNewTarget();

        lastJumpTime = -jumpCooldown; // Initialize to allow immediate first jump

        // Add and configure the AgentTargetingLine component
        targetingLine = gameObject.AddComponent<AgentTargetingLine>();
        targetingLine.SetColor(Random.ColorHSV()); // Set a random color for each bee

        // Add and configure the AgentPathLine component
        pathLine = gameObject.AddComponent<AgentPathLine>();
        //Debug.Log("BeeAgent: AgentPathLine component added.");

        
    }

    private void OnEnable()
    {
        activeBeeAgents.Add(this);
    }

    private void OnDisable()
    {
        activeBeeAgents.Remove(this);
    }

    void Update()
    {
        if (!GameManager.instance.EmbeddingsLoaded)
        {
            Debug.Log("Embeddings not loaded yet.");
            return;
        }
        if (energy > 0)
        {
            if (target == null)
            {
                FindNewTarget();
            }

            if (target != null)
            {
                MoveTowardsTarget();
                targetingLine.target = target; // Update the target for the targeting line
            }

            ConsumeEnergy();
        }
        else
        {
            Die();
        }

        // Keep the bee upright
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        // Limit the bee's height dynamically based on the target's height
        LimitHeight();
    }

    void MoveTowardsTarget()
    {
        if (Time.time - lastJumpTime >= jumpCooldown)
        {
            Vector3 direction = (target.position - transform.position).normalized;

            // Adjust the jump force based on the distance to the target
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            float jumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, distanceToTarget / 10f); // Adjust the divisor based on your scene's scale

            Vector3 jumpVector = new Vector3(direction.x * speed, jumpForce, direction.z * speed);
            rb.AddForce(jumpVector, ForceMode.VelocityChange);

            lastJumpTime = Time.time;
        }
    }

    void ConsumeEnergy()
    {
        energy -= energyConsumptionRate * Time.deltaTime;
    }

    void Die()
    {
        rb.velocity = Vector3.zero;
        this.enabled = false;
        Debug.Log("Bee has died.");
        activeBeeAgents.Remove(this);
        Destroy(gameObject);
    }

    public void RegisterFruitCollision(Vector3 position)
    {
        // Save the position where the fruit was caught
        pathLine.AddPathPosition(position);
        //Debug.Log($"BeeAgent: Registered fruit collision at position {position}. Path position added.");
    }

    void FindNewTarget()
    {
        GameObject[] fruits = GameObject.FindGameObjectsWithTag("Fruit");
        //Debug.Log("Fruits found: " + fruits.Length);
        if (fruits.Length > 0)
        {
            float bestScore = float.MinValue;
            GameObject bestFruit = null;

            foreach (GameObject fruit in fruits)
            {
                BeeCollectible collectible = fruit.GetComponent<BeeCollectible>();
                string word = collectible?.GetWord();
                //Debug.Log("Collectible: " + word );

                if (!string.IsNullOrEmpty(word))
                {
                    float cosineDistance = 1 - GameManager.CosineSimilarity(GameManager.GetEmbedding(beeWord), GameManager.GetEmbedding(collectible.GetWord()));

                    // Use the cosine distance directly as the score
                    float score = 1 - cosineDistance;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestFruit = fruit;
                    }
                }
            }

            if (bestFruit != null)
            {
                target = bestFruit.transform;
            }
            else
            {
                target = null;
            }
        }
        else
        {
            target = null;
        }
    }

    void LimitHeight()
    {
        if (target != null)
        {
            float targetHeight = target.position.y;
            if (transform.position.y > targetHeight + 1f) // Adding a small buffer
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                transform.position = new Vector3(transform.position.x, targetHeight + 1f, transform.position.z);
            }
        }
    }
}




public class AgentTargetingLine : MonoBehaviour
{
    public Transform target; // Assign the target transform in the Inspector
    private LineRenderer lineRenderer;

    void Start()
    {
        // Add a LineRenderer component if not already added
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Set LineRenderer properties
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Use a default material or assign your own
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.positionCount = 0; // Initialize with no positions
    }

    void Update()
    {
        if (target != null)
        {
            if (lineRenderer.positionCount < 2)
            {
                lineRenderer.positionCount = 2;
            }
            // Set the positions of the line
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, target.position);
        }
    }

    public void SetColor(Color color)
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
        }
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }
}

public class AgentPathLine : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private List<Vector3> pathPositions = new List<Vector3>();

    void Start()
    {
        // Create a new child GameObject for the path line
        GameObject pathLineObject = new GameObject("PathLine");
        pathLineObject.transform.SetParent(transform);

        // Add a LineRenderer component to the child GameObject
        lineRenderer = pathLineObject.AddComponent<LineRenderer>();

        // Set LineRenderer properties
        lineRenderer.startWidth = 1f;
        lineRenderer.endWidth = 1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Use a default material or assign your own
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.positionCount = 0; // Initialize with no positions

        //Debug.Log("AgentPathLine: LineRenderer initialized.");
    }

    public void AddPathPosition(Vector3 position)
    {
        // Adjust the Y-coordinate to be higher (e.g., y = 20)
        Vector3 adjustedPosition = new Vector3(position.x, 30, position.z);
        pathPositions.Add(adjustedPosition);

        // Update the LineRenderer with the new path positions
        lineRenderer.positionCount = pathPositions.Count;
        lineRenderer.SetPositions(pathPositions.ToArray());

        //Debug.Log($"AgentPathLine: Added path position {adjustedPosition}. Total positions: {pathPositions.Count}");
    }
}