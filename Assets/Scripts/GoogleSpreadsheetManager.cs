using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class GoogleSheetsManager : MonoBehaviour
{
    private static GoogleSheetsManager _instance;
    public static GoogleSheetsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GoogleSheetsManager>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(GoogleSheetsManager).Name);
                    _instance = singletonObject.AddComponent<GoogleSheetsManager>();
                    DontDestroyOnLoad(singletonObject);
                    Debug.Log("GoogleSheetsManager was not found, a new one was created.");
                }
            }
            return _instance;
        }
    }

    public string accessToken;
    private string spreadsheetId = "1FOAmGBrqS8n0QS9hmFCy7ZF0z6KLb8OioV7SV5fZYJo";

    public void Initialize(string token, string sheetId)
    {
        accessToken = token;
        spreadsheetId = sheetId;
        Debug.Log("GoogleSheetsManager initialized with access token: " + accessToken);
        Debug.Log("GoogleSheetsManager initialized with spreadsheet ID: " + spreadsheetId);
    }

    public IEnumerator WriteDataToGoogleSheets(List<object> rowData, string sheetName)
    {
        if (string.IsNullOrEmpty(spreadsheetId))
        {
            Debug.LogError("Spreadsheet ID is null or empty");
            yield break;
        }

        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("Access token is null or empty");
            yield break;
        }

        var payload = new GoogleSheetsPayload
        {
            Values = new List<IList<object>> { rowData }
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{sheetName}!A1:append?valueInputOption=USER_ENTERED";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("AppendRequest executed successfully. Response: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error in WriteDataToGoogleSheets: " + request.error);
            Debug.LogError("Response Code: " + request.responseCode);
            Debug.LogError("Error Message: " + request.downloadHandler.text);
        }
    }

    public string GetAccessToken()
    {
        return accessToken;
    }
}