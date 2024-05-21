using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // Make sure to include this at the top of your script

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using Newtonsoft.Json;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;


[System.Serializable]
public class GameParameters
{
    public float energyDepletionRate;
    public float energyRecoveryPerTargetFruit;
    public float rotationSpeed = 10f;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    private static Dictionary<string, float[]> embeddings;
    public static List<string> collectedWords = new List<string>();

    private GameParameters gameParameters;

    public Light directionalLight;
    public float rotationSpeed = 10f;
    private float currentRotation = 0f;

    //Scoring system
    public string targetWord = "good";
    public string avoidWord = "bad";
    public float wordSelectionThreshold = 0.5f; // Percentage of the dictionary to select words from

    public long maxPointsPerWord = 1000;
    public static long score = 0;

    public float maxEnergy = 100f;
    public float currentEnergy;
    public float energyDepletionRate = 1f;
    public float energyRecoveryPerTargetFruit = 20f;

    private bool gameOverCalled = false;

    public string mainMenuScene = "Menu";

    public bool embeddingsLoaded = false;

    public Button playButton;


    //Canvases
    public GameObject gameOverUI = null;
    public GameObject canvasCentral;
    public GameObject canvasHelp;


    //Highscore spreadsheet
    private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
    private static readonly string ApplicationName = "Fractal Tree Orchard";
    private SheetsService sheetsService;
    public string spreadsheetId = "1FOAmGBrqS8n0QS9hmFCy7ZF0z6KLb8OioV7SV5fZYJo";
    private string range = "Sheet1!A1:D";  // Adjust the range as needed
    private string accessToken; // Store the access token

    private List<string> highScores = new List<string>();
    public TextMeshProUGUI highScoresText;
    private float highScoreUpdateInterval = 0.5f;
    private float highScoreUpdateTimer = 0f;
    public float scrollSpeed = 20f; // Speed at which the text scrolls




    public void AToggleHelp()
    {
        if (canvasCentral != null && canvasHelp != null)
        {
            // Toggle the active state of both canvases
            canvasCentral.SetActive(!canvasCentral.activeSelf);
            canvasHelp.SetActive(!canvasHelp.activeSelf);
        }
    }

    private IEnumerator InitializeGoogleSheets()
    {
        // Load Google credentials and get the access token
        yield return StartCoroutine(LoadGoogleCredentials());
    }

    private IEnumerator LoadGoogleCredentials()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "fractaltreeorchard-highscores-77d4e8fb7122.json");
        string url = "file://" + filePath;

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonContent = www.downloadHandler.text;
                JsonCredentialParameters credentials = JsonConvert.DeserializeObject<JsonCredentialParameters>(jsonContent);
                credentials.PrivateKey = credentials.PrivateKey.Replace("\\n", "\n");

                // Get the access token
                var initializer = new ServiceAccountCredential.Initializer(credentials.ClientEmail)
                {
                    ProjectId = credentials.ProjectId,
                    KeyId = credentials.PrivateKeyId,
                    Scopes = new[] { SheetsService.Scope.Spreadsheets }
                }.FromPrivateKey(credentials.PrivateKey);

                var serviceAccountCredential = new ServiceAccountCredential(initializer);

                if (serviceAccountCredential == null)
                {
                    Debug.LogError("ServiceAccountCredential is null.");
                    yield break;
                }

                if (serviceAccountCredential.Token == null)
                {
                    Debug.LogError("ServiceAccountCredential.Token is null.");
                    yield break;
                }

                accessToken = serviceAccountCredential.Token.AccessToken;

                if (string.IsNullOrEmpty(accessToken))
                {
                    Debug.LogError("Access token is null or empty.");
                    yield break;
                }

                Debug.Log("Access token successfully obtained.");
            }
            else
            {
                Debug.LogError("Failed to load Google credentials: " + www.error);
            }
        }
    }


 private IEnumerator GetAccessToken()
{
    string filePath = Path.Combine(Application.streamingAssetsPath, "fractaltreeorchard-highscores-77d4e8fb7122.json");
    string url;

    if (Application.platform == RuntimePlatform.WebGLPlayer)
    {
        // For WebGL, use the URL directly
        url = filePath;
    }
    else
    {
        // For other platforms, use the file:// prefix
        url = "file://" + filePath;
    }

    Debug.Log("Credentials file URL: " + url);

    using (UnityWebRequest www = UnityWebRequest.Get(url))
    {
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string jsonContent = www.downloadHandler.text;
            JsonCredentialParameters credentials = JsonConvert.DeserializeObject<JsonCredentialParameters>(jsonContent);
            credentials.PrivateKey = credentials.PrivateKey.Replace("\\n", "\n");

            // Extract the Base64-encoded key data from the PEM format
            string privateKeyPem = credentials.PrivateKey;
            string privateKeyBase64 = privateKeyPem
                .Replace("-----BEGIN PRIVATE KEY-----", "")
                .Replace("-----END PRIVATE KEY-----", "")
                .Replace("\n", "")
                .Replace("\r", "");

            // Create the JWT assertion
            var header = new { alg = "RS256", typ = "JWT" };
            var claimSet = new
            {
                iss = credentials.ClientEmail,
                scope = "https://www.googleapis.com/auth/spreadsheets",
                aud = "https://oauth2.googleapis.com/token",
                exp = ((DateTimeOffset)DateTime.UtcNow.AddHours(1)).ToUnixTimeSeconds(),
                iat = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds()
            };

            string headerEncoded = Base64UrlEncode(JsonConvert.SerializeObject(header));
            string claimSetEncoded = Base64UrlEncode(JsonConvert.SerializeObject(claimSet));
            string unsignedToken = $"{headerEncoded}.{claimSetEncoded}";

            // Use BouncyCastle to sign the JWT
            AsymmetricKeyParameter privateKey;
            using (var reader = new StringReader(privateKeyPem))
            {
                var pemReader = new PemReader(reader);
                var keyObject = pemReader.ReadObject();

                if (keyObject is AsymmetricCipherKeyPair keyPair)
                {
                    privateKey = keyPair.Private;
                }
                else if (keyObject is AsymmetricKeyParameter keyParameter)
                {
                    privateKey = keyParameter;
                }
                else
                {
                    Debug.LogError("Unsupported key type.");
                    yield break;
                }
            }

            var signer = SignerUtilities.GetSigner("SHA256withRSA");
            signer.Init(true, privateKey);
            signer.BlockUpdate(Encoding.UTF8.GetBytes(unsignedToken), 0, unsignedToken.Length);
            byte[] signature = signer.GenerateSignature();
            string signedToken = $"{unsignedToken}.{Base64UrlEncode(signature)}";

            // Create the request body
            var requestBody = new Dictionary<string, string>
            {
                { "grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
                { "assertion", signedToken }
            };

            // Make the request to the token endpoint
            string tokenUrl = "https://oauth2.googleapis.com/token";
            Debug.Log("Token request URL: " + tokenUrl);

            using (UnityWebRequest tokenRequest = UnityWebRequest.Post(tokenUrl, requestBody))
            {
                yield return tokenRequest.SendWebRequest();

                if (tokenRequest.result == UnityWebRequest.Result.Success)
                {
                    var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(tokenRequest.downloadHandler.text);
                    accessToken = tokenResponse["access_token"];
                    Debug.Log("Access token successfully obtained.");
                    StartCoroutine(FetchHighScores());
                }
                else
                {
                    Debug.LogError("Failed to obtain access token: " + tokenRequest.error);
                }
            }
        }
        else
        {
            Debug.LogError("Failed to load Google credentials: " + www.error);
        }
    }
}

private string Base64UrlEncode(string input)
{
    var bytes = Encoding.UTF8.GetBytes(input);
    return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

private string Base64UrlEncode(byte[] input)
{
    return Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}


private IEnumerator FetchHighScores()
{
    Debug.Log("Fetching high scores...");
    Debug.Log("Access Token: " + accessToken);

    // Check if accessToken is null or empty
    if (string.IsNullOrEmpty(accessToken))
    {
        Debug.LogError("Access token is null or empty. Cannot fetch high scores.");
        yield break;
    }

    // Create the request URL
    string url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{range}?access_token={accessToken}";
    Debug.Log("Request URL: " + url);

    // Create the UnityWebRequest
    UnityWebRequest request = UnityWebRequest.Get(url);
    request.SetRequestHeader("Authorization", "Bearer " + accessToken);

    // Send the request
    yield return request.SendWebRequest();

    if (request.result == UnityWebRequest.Result.Success)
    {
        Debug.Log("High scores fetched successfully.");
        string jsonResponse = request.downloadHandler.text;
        Debug.Log("JSON Response: " + jsonResponse); // Add this line to log the JSON response

        var response = JsonConvert.DeserializeObject<GoogleSheetsResponse>(jsonResponse);
        List<IList<object>> values = response.Values;

        // Log the number of rows fetched
        Debug.Log("Number of rows fetched: " + values.Count);

        // Process the high scores
        highScores.Clear();
        foreach (var row in values)
        {
            Debug.Log("Row data: " + string.Join(", ", row)); // Add this line to log each row's data

            if (row.Count >= 4) // Ensure the row has at least 4 elements
            {
                string essence = row[0].ToString();
                string targetWord = row[1].ToString();
                string avoidWord = row[2].ToString();
                string score = row[3].ToString();
                highScores.Add($"{essence}: {targetWord}: {avoidWord}: {score}");
            }
            else
            {
                Debug.LogWarning("Row does not have enough elements: " + string.Join(", ", row));
            }
        }

        // Sort the high scores from highest to lowest
        highScores = highScores.OrderByDescending(hs => 
        {
            string[] parts = hs.Split(':');
            return long.Parse(parts[3].Trim());
        }).ToList();

        Debug.Log("High scores processed and sorted: " + string.Join(", ", highScores));
        DisplayHighScores();
    }
    else
    {
        Debug.LogError("Failed to fetch high scores: " + request.error);
        Debug.LogError("Response Code: " + request.responseCode);
        Debug.LogError("Error Message: " + request.downloadHandler.text);
    }
}

    private string FormatHighScore(string essence, string targetWord, string avoidWord, string score)
    {
        return $"{essence}    <color=red>{avoidWord}</color>-<color=green>{targetWord}</color>    {score}";
    }
    private void DisplayHighScores()
    {
        if (highScoresText == null)
        {
            Debug.LogError("HighScoresText is not assigned.");
            return;
        }

        StringBuilder displayText = new StringBuilder();
        foreach (string highScore in highScores)
        {
            // Split the high score entry into its components
            string[] parts = highScore.Split(':');
            if (parts.Length == 4)
            {
                string essence = parts[0].Trim();
                string targetWord = parts[1].Trim();
                string avoidWord = parts[2].Trim();
                string score = parts[3].Trim();

                // Format the high score entry
                displayText.AppendLine(FormatHighScore(essence, targetWord, avoidWord, score));
            }
        }

        highScoresText.text = displayText.ToString();

        // Reset the position of the text to the bottom of the screen
        highScoresText.transform.localPosition = new Vector3(highScoresText.transform.localPosition.x, -Screen.height/2, highScoresText.transform.localPosition.z);
    }

    private IEnumerator LoadGameParameters()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "GameParameters.json");
            string url = filePath;

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string json = www.downloadHandler.text;
                    gameParameters = JsonUtility.FromJson<GameParameters>(json);
                    ApplyGameParameters();
                }
                else
                {
                    Debug.LogError("Failed to load game parameters: " + www.error);
                }
            }
        }
    }

    private void ApplyGameParameters()
    {
        if (gameParameters != null)
        {
            energyDepletionRate = gameParameters.energyDepletionRate;
            energyRecoveryPerTargetFruit = gameParameters.energyRecoveryPerTargetFruit;
            rotationSpeed = gameParameters.rotationSpeed;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            if (gameOverUI != null)
            {
                DontDestroyOnLoad(gameOverUI);
                gameOverUI.SetActive(false); // Hide the game over UI initially
            }
            DontDestroyOnLoad(gameObject);


            StartCoroutine(LoadEmbeddings());
            InitializeGoogleSheets();
            
            SceneManager.sceneLoaded += OnSceneLoaded;
            currentEnergy = maxEnergy;
            //Debug.Log($"Initial Energy: {currentEnergy}");
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                StartCoroutine(LoadGameParameters());
            }


        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    Debug.Log(scene.name);


}


    private void EnablePlayButton()
        {
            if (playButton != null)
            {
                playButton.interactable = true;
                Debug.Log("Play button enabled.");
            }
        }

    private IEnumerator LoadEmbeddings()
    {
        embeddings = new Dictionary<string, float[]>();

        string[] fileNames = {"glove.6B.50d.95MB.txt", "glove.6B.50d.txt", "improved_supercompact.txt" };

        foreach (string fileName in fileNames)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // WebGL build
                string url = filePath;

                using (UnityWebRequest www = UnityWebRequest.Get(url))
                {
                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        ProcessEmbeddings(www.downloadHandler.text);
                        Debug.Log($"Embeddings file '{fileName}' loaded successfully.");
                        embeddingsLoaded = true;
                        EnablePlayButton();
                        yield break;
                    }
                }
            }
            else
            {
                // Local build
                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);
                    ProcessEmbeddings(string.Join("\n", lines));
                    Debug.Log($"Embeddings file '{fileName}' loaded successfully.");
                    embeddingsLoaded = true;
                    EnablePlayButton();
                    yield break;
                }
            }
        }

        Debug.LogError("No embeddings file found.");
    }

    private void ProcessEmbeddings(string embeddingsText)
    {
        string[] lines = embeddingsText.Split('\n');

        foreach (string line in lines)
        {
            string[] parts = line.Split(' ');
            string word = parts[0];
            float[] embedding = new float[parts.Length - 1];

            for (int i = 1; i < parts.Length; i++)
            {
                embedding[i - 1] = float.Parse(parts[i]);
            }

            embeddings[word] = embedding;
        }

        Debug.Log("GloVe embeddings loaded successfully.");
    }

    public static float[] GetEmbedding(string word)
    {
        if (embeddings.ContainsKey(word))
        {
            return embeddings[word];
        }
        return null;
    }

    

    private Light FindDirectionalLight()
    {
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                return light;
            }
        }
        return null;
    }

    public Quaternion GetDirectionalLightRotation()
{
    if (directionalLight != null)
    {
        return directionalLight.transform.rotation;
    }
    return Quaternion.identity;
}


    private void GameOver()
    {
        // Assuming gameOverUI is the root of your UI hierarchy, e.g., CanvasGameOver
        if (gameOverUI != null)
        {
            // Activate the Game Over UI
            gameOverUI.SetActive(true);
        }

        // Disable player movement
        MovementComponent movementComponent = FindObjectOfType<MovementComponent>();
        if (movementComponent != null)
        {
            movementComponent.enabled = false;
        }

        // Calculate the average embedding and find the closest word
        float[] averageEmbedding = CalculateAverageEmbedding();
        if (averageEmbedding != null)
        {
            string finalEssence = FindClosestWord(averageEmbedding);
            if (!string.IsNullOrEmpty(finalEssence))
            {
                // Use the full path to find the FinalEssenceText GameObject
                Transform finalEssenceTextTransform = gameOverUI.transform.Find("/CanvasGameOver/GameOverMenu/Essence/Wood/FinalEssenceText");
                if (finalEssenceTextTransform != null)
                {
                    TextMeshProUGUI finalEssenceText = finalEssenceTextTransform.GetComponent<TextMeshProUGUI>();

                    if (finalEssenceText != null)
                    {
                        StartCoroutine(WriteHighScoreToGoogleSheets(finalEssence, targetWord, avoidWord, score, spreadsheetId));
                        finalEssenceText.text = $"{finalEssence}";
                        Debug.Log("Final Essence Text: " + finalEssence);
                    }
                    else
                    {
                        Debug.LogError("FinalEssenceText GameObject does not have a TextMeshProUGUI component.");
                    }
                }
                else
                {
                    Debug.LogError("Could not find FinalEssenceText GameObject.");
                }
            }
        }
    }

 private IEnumerator WriteHighScoreToGoogleSheets(string essence, string targetWord, string avoidWord, long score, string spreadsheetId)
    {
        Debug.Log("Starting WriteHighScoreToGoogleSheets");

        // Check if spreadsheetId is null or empty
        if (string.IsNullOrEmpty(spreadsheetId))
        {
            Debug.LogError("Spreadsheet ID is null or empty");
            yield break;
        }

        Debug.Log("Spreadsheet ID: " + spreadsheetId);

        // Create a new row with the essence, targetWord, avoidWord, and score
        var newRow = new List<object>() { essence, targetWord, avoidWord, score };
        Debug.Log("Created new row: " + string.Join(", ", newRow));

        // Create a GoogleSheetsPayload object and set its values
        var payload = new GoogleSheetsPayload
        {
            Values = new List<IList<object>> { newRow }
        };

        Debug.Log("Created GoogleSheetsPayload with values: " + JsonConvert.SerializeObject(payload.Values));

        // Serialize the GoogleSheetsPayload object to JSON
        string jsonPayload = JsonConvert.SerializeObject(payload);
        Debug.Log("JSON Payload: " + jsonPayload);

        // Create the request URL
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{range}:append?valueInputOption=USER_ENTERED";
        Debug.Log("Request URL: " + url);

        // Create the UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        // Send the request
        yield return request.SendWebRequest();

        try
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("AppendRequest executed successfully. Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error in WriteHighScoreToGoogleSheets: " + request.error);
                Debug.LogError("Response Code: " + request.responseCode);
                Debug.LogError("Error Message: " + request.downloadHandler.text);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in WriteHighScoreToGoogleSheets: " + ex.Message);
            Debug.LogError("Stack Trace: " + ex.StackTrace);
        }
    }


    

    private void Start()
    {
        SelectRandomWords();
        // Set the initial state of the canvases
        if (canvasCentral != null && canvasHelp != null)
        {
            canvasCentral.SetActive(true);
            canvasHelp.SetActive(false);
        }
        else
        {
            Debug.LogError("CanvasCentral or CanvasHelp not assigned in the Inspector.");
        }
        StartCoroutine(GetAccessToken());
        
    }

    private void SelectRandomWords()
    {
        if (embeddings != null && embeddings.Count > 0)
          {
            List<string> keys = new List<string>(embeddings.Keys);
            int thresholdCount = Mathf.FloorToInt(keys.Count * wordSelectionThreshold);

            int randomTargetIndex = UnityEngine.Random.Range(0, thresholdCount);
            targetWord = keys[randomTargetIndex];

            // Ensure the Avoid word is different from the Target word
            string avoidWordCandidate;
            do
            {
                int randomAvoidIndex = UnityEngine.Random.Range(0, thresholdCount);
                avoidWordCandidate = keys[randomAvoidIndex];
            } while (avoidWordCandidate == targetWord);

            avoidWord = avoidWordCandidate;
        }
    }

    public void RestartGame()
    {
        // Reset variables and state
        currentEnergy = maxEnergy;
        score = 0;
        collectedWords.Clear();
        gameOverCalled = false;
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false); // Hide the game over UI
        }

        // Select new random words for the next game
        SelectRandomWords();

        // Reload the current scene to restart the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private float[] CalculateAverageEmbedding()
    {
        if (collectedWords.Count == 0) return null;

        float[] sumEmbedding = new float[embeddings[collectedWords[0]].Length];
        foreach (string word in collectedWords)
        {
            float[] wordEmbedding = embeddings[word];
            for (int i = 0; i < sumEmbedding.Length; i++)
            {
                sumEmbedding[i] += wordEmbedding[i];
            }
        }

        for (int i = 0; i < sumEmbedding.Length; i++)
        {
            sumEmbedding[i] /= collectedWords.Count;
        }

        Debug.Log("Average Embedding: " + string.Join(", ", sumEmbedding));
        return sumEmbedding;
    }

    private string FindClosestWord(float[] averageEmbedding)
    {
        float maxSimilarity = float.NegativeInfinity;
        string closestWord = null;

        foreach (var pair in embeddings)
        {
            float similarity = CosineSimilarity(averageEmbedding, pair.Value);
            if (similarity > maxSimilarity)
            {
                maxSimilarity = similarity;
                closestWord = pair.Key;
            }
        }

        Debug.Log("Closest Word: " + closestWord + ", Similarity: " + maxSimilarity);
        return closestWord;
    }

    private void Update()
    {
        // ...
        if (directionalLight == null)
        {
            directionalLight = FindDirectionalLight();
        }

        if (directionalLight != null)
        {
            currentRotation += rotationSpeed * Time.deltaTime;
            if (currentRotation >= 360f)
            {
                currentRotation -= 360f;
            }
            directionalLight.transform.rotation = Quaternion.Euler(currentRotation, -30f, 0f);
        }

        if (SceneManager.GetActiveScene().name != mainMenuScene)
        {
        // Update energy and check for game over only in the game scene
        currentEnergy -= energyDepletionRate * Time.deltaTime;
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        //Debug.Log($"Current Energy: {currentEnergy}");

        if (currentEnergy <= 0f && !gameOverCalled)
            {
                GameOver();
                gameOverCalled = true;
            }
        }
        if (highScoresText != null)
        {
            // Move the text upwards
            highScoresText.transform.Translate(Vector3.up * scrollSpeed * Time.deltaTime);

            // Optionally, reset the position if it goes too far
            if (highScoresText.transform.localPosition.y > Screen.height)
            {
                highScoresText.transform.localPosition = new Vector3(highScoresText.transform.localPosition.x, -Screen.height, highScoresText.transform.localPosition.z);
            }
        }   
    }

    public static string[] GetAllWords()
    {
        if (embeddings != null)
        {
            return embeddings.Keys.ToArray();
        }
        return null;
    }

    public static long CalculatePoints(string word)
    {
        float targetSimilarity = CosineSimilarity(GetEmbedding(word), GetEmbedding(instance.targetWord));
        float avoidSimilarity = CosineSimilarity(GetEmbedding(word), GetEmbedding(instance.avoidWord));

        // Determine which word is closer based on similarity
        bool isCloserToTarget = targetSimilarity > avoidSimilarity;

        // Calculate the score based on the winning similarity
        float winningSimilarity = isCloserToTarget? targetSimilarity : avoidSimilarity;
        long score = (long)(winningSimilarity * instance.maxPointsPerWord);

        // Adjust the score based on whether it's closer to the target or avoid word
        if (isCloserToTarget)
        {
            // If closer to the target, ensure the score is positive
            score = (long)Mathf.Abs(score);
        }
        else
        {
            // If closer to the avoid word, make the score negative
            score = -(long)Mathf.Abs(score);
        }

        // Ensure the score is within a reasonable range

        return score;
    }

    public static void AddCollectedWord(string word)
    {
        if (!string.IsNullOrEmpty(word) && !collectedWords.Contains(word))
        {
            collectedWords.Add(word);
            long points = CalculatePoints(word);
            score += points;

            if (points > 0)
            {
                instance.currentEnergy += instance.energyRecoveryPerTargetFruit;
                instance.currentEnergy = Mathf.Clamp(instance.currentEnergy, 0f, instance.maxEnergy);
                //Debug.Log($"Energy after collecting target fruit: {instance.currentEnergy}");
                
            }

            //Debug.Log($"Collected word: {word}, Points: {points}, Total Score: {score}, Energy: {instance.currentEnergy}");
        }
    }

    public static string GetCollectedWordsText()
    {
        return string.Join("\n", collectedWords);
    }

    

    public static float CosineSimilarity(float[] vector1, float[] vector2)
    {
        float dotProduct = 0.0f;
        float magnitude1 = 0.0f;
        float magnitude2 = 0.0f;

        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        magnitude1 = Mathf.Sqrt(magnitude1);
        magnitude2 = Mathf.Sqrt(magnitude2);

        if (magnitude1 != 0.0f && magnitude2 != 0.0f)
        {
            return dotProduct / (magnitude1 * magnitude2);
        }
        else
        {
            return 0.0f;
        }
    }

    public static string[] GetSimilarWords(string word, int count = 5, float threshold = 0.1f)
    {
        if (embeddings.ContainsKey(word))
        {
            float[] wordVector = embeddings[word];
            var similarWords = embeddings
                .Where(pair => pair.Key != word)
                .Select(pair => new { Word = pair.Key, Similarity = CosineSimilarity(wordVector, pair.Value) })
                .Where(item => item.Similarity >= threshold)
                .OrderByDescending(item => item.Similarity)
                .Take(count)
                .Select(item => item.Word)
                .ToArray();

            return similarWords;
        }

        return new string[0];
    }
}

[Serializable]
public class JsonCredentialParameters
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("project_id")]
    public string ProjectId { get; set; }

    [JsonProperty("private_key_id")]
    public string PrivateKeyId { get; set; }

    [JsonProperty("private_key")]
    public string PrivateKey { get; set; }

    [JsonProperty("client_email")]
    public string ClientEmail { get; set; }

    [JsonProperty("client_id")]
    public string ClientId { get; set; }

    [JsonProperty("auth_uri")]
    public string AuthUri { get; set; }

    [JsonProperty("token_uri")]
    public string TokenUri { get; set; }

    [JsonProperty("auth_provider_x509_cert_url")]
    public string AuthProviderX509CertUrl { get; set; }

    [JsonProperty("client_x509_cert_url")]
    public string ClientX509CertUrl { get; set; }
}

public class GoogleSheetsPayload
{
    [JsonProperty("values")]
    public List<IList<object>> Values { get; set; }
}

public class GoogleSheetsResponse
{
    [JsonProperty("values")]
    public List<IList<object>> Values { get; set; }
}