using UnityEngine;
using TMPro;
public class FractalTree : MonoBehaviour
{
    public int maxDepth = 8;
    public float childDiameterScale = 0.8f; // Child scale factor for the diameter
    public float childHeightScale = 0.8f; // Child scale factor for the height
    public float branchAngle = 66f;
    public int branches = 2;
    private int depth;

    public float rotationSpeed = 3f;
    public float oscillationSpeed = 1f; // Speed of the oscillation
    public float oscillationAmplitude = 0.5f; // Amplitude of the oscillation
    private Vector3 initialLocalPosition;

    public bool randomizeAxisRotation = false;


    public Material trunkMaterial; // Material for the trunk
    public Material leafMaterial; // Material for the leaves

    public bool hasFruit = false;
    public float fruitProbability = 0.5f;
    public GameObject fruitPrefab;
    public string baseWord = "banana";


    void Start()
    {
        if (depth == 0)
        {
            //Debug.Log("root");
            CreateBranches();
        }
        initialLocalPosition = transform.localPosition;
    }

    Vector3 CalculateGlobalScale(Transform transform)
    {
        Vector3 globalScale = transform.localScale;
        Transform parent = transform.parent;

        while (parent != null)
        {
            globalScale = Vector3.Scale(globalScale, parent.localScale);
            parent = parent.parent;
        }

        return globalScale;
    }
public void Initialize(int depth, Vector3 position, Quaternion rotation, Material trunkMaterial, Material leafMaterial, int maxDepth, float childDiameterScale, float childHeightScale, float branchAngle, int branches, bool randomizeAxisRotation, bool hasFruit, float fruitProbability, GameObject fruitPrefab, string baseWord)
    {
        this.depth = depth;
        this.trunkMaterial = trunkMaterial;
        this.leafMaterial = leafMaterial;
        this.maxDepth = maxDepth;
        this.childDiameterScale = childDiameterScale;
        this.childHeightScale = childHeightScale;
        this.branchAngle = branchAngle;
        this.branches = branches;
        this.randomizeAxisRotation = randomizeAxisRotation;
        this.hasFruit = hasFruit;
        this.fruitProbability = fruitProbability;
        this.fruitPrefab = fruitPrefab;

        transform.position = position + rotation * Vector3.up * CalculateGlobalScale(this.transform).y;
        transform.rotation = rotation;

        this.baseWord = baseWord;

        if (depth < this.maxDepth)
        {
            CreateBranches();
        }
    }

void Update()
    {
        if (depth %2 == 0)
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.Self);
        else
            transform.Rotate(Vector3.up * -2* rotationSpeed * Time.deltaTime, Space.Self);
    }

    public void ExportTreeInfoToJSON()
    {
    string jsonString = JsonUtility.ToJson(this);
    GUIUtility.systemCopyBuffer = jsonString;
    }

    private string GetRandomWordFromEmbeddings()
    {
        // Get all the words from the embeddings
        string[] words = GameManager.GetAllWords();

        if (words != null && words.Length > 0)
        {
            // Select a random word from the array
            int randomIndex = Random.Range(0, words.Length);
            return words[randomIndex];
        }

        return string.Empty;
    }

    private string GetSimilarWordToBaseWord(int count = 15)
    {
        // Number of similar words to consider
        string[] similarWords = GameManager.GetSimilarWords(baseWord, count);
        if (similarWords.Length > 0)
        {
            //Debug.Log($"Base Word: {baseWord}, {similarWords.Length} similar Words: {string.Join(", ", similarWords)}");
            int randomIndex = Random.Range(0, similarWords.Length);
            return similarWords[randomIndex];
        }
        return string.Empty;
    }

    private void CreateBranches()
    {
        bool shouldSpawnFruit = false;

        if (depth == maxDepth - 2 && hasFruit)
        {
            float randomValue = Random.value;
            //Debug.Log($"Depth: {depth}, Random Value: {randomValue}, Fruit Probability: {fruitProbability}");

            if (randomValue < fruitProbability)
            {
                //Debug.Log("DENTRO");
                shouldSpawnFruit = true;
            }
        }

        if (shouldSpawnFruit)
        {
            // Spawn a fruit
            //Debug.Log("Fruit");
            Vector3 fruitPosition = transform.position + transform.up * transform.localScale.y * 1.25f;
            GameObject fruit = Instantiate(fruitPrefab, fruitPosition, Quaternion.identity);
            fruit.transform.SetParent(transform, true);

            // Get the TextMeshPro component from the child object named "FruitText"
            TextMeshPro textMeshPro = fruit.transform.Find("FruitText").GetComponent<TextMeshPro>();
            if (textMeshPro != null)
            {
                // Get a random word from the embeddings
                // Get a similar word to the base word from the embeddings
                string similarWord = GetSimilarWordToBaseWord(80);
                textMeshPro.text = similarWord;
            }
            else
            {
                Debug.LogWarning("TextMeshPro component not found on FruitText child object!");
            }
        }
        else
        {
            for (int i = 0; i < branches; i++)
            {
                GameObject childObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                childObj.transform.SetParent(this.transform, false);

                if (depth > 0)
                {
                    // Destroy(childObj.GetComponent<Collider>());
                }

                Vector3 childScaleVector = new Vector3(childDiameterScale, childHeightScale, childDiameterScale);
                childObj.transform.localScale = childScaleVector;
                float parentHeight = transform.localScale.y;
                childObj.transform.localPosition = new Vector3(0, parentHeight, 0);

                float angle = ((float)i / branches) * 360f;
                childObj.transform.localRotation = Quaternion.Euler(branchAngle, angle, 0);

                // Apply random rotation around the up axis if enabled
                if (randomizeAxisRotation)
                {
                    float randomRotation = Random.Range(-15f, 15f); // Random rotation between -15 and 15 degrees
                    childObj.transform.Rotate(Vector3.up, randomRotation);
                }

                if (depth == maxDepth - 1)
                {
                    // Spawn a leaf
                    //Debug.Log("Leaf");
                    childObj.GetComponent<Renderer>().material = leafMaterial;
                }
                else
                {
                    childObj.GetComponent<Renderer>().material = trunkMaterial;
                }

                if (depth < maxDepth - 1)
                {
                    FractalTree childFractal = childObj.AddComponent<FractalTree>();
                    childFractal.Initialize(depth + 1, childObj.transform.position, childObj.transform.rotation, trunkMaterial, leafMaterial, maxDepth, childDiameterScale, childHeightScale, branchAngle, branches, randomizeAxisRotation, hasFruit, fruitProbability, fruitPrefab, baseWord);
                }
            }
        }
    }


}