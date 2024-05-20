using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.Threading;

public class GoogleSheetsTest : MonoBehaviour
{
    private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
    private static readonly string ApplicationName = "Your Application Name";
    private SheetsService service;

    void Start()
    {
        InitializeGoogleSheets();
        ReadSheetDataAndAddTime();
    }

    private void InitializeGoogleSheets()
    {
        GoogleCredential credential;

        using (var stream = new FileStream(Path.Combine(Application.streamingAssetsPath, "fractaltreeorchard-highscores-77d4e8fb7122.json"), FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
        }

        service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
    }

    private void ReadSheetDataAndAddTime()
    {
        string spreadsheetId = "1FOAmGBrqS8n0QS9hmFCy7ZF0z6KLb8OioV7SV5fZYJo";
        string range = "Sheet1!A1:D";  // Adjust the range as needed

        // Read the current contents of the spreadsheet
        var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
        var response = request.Execute();
        IList<IList<object>> values = response.Values;

        if (values != null && values.Count > 0)
        {
            foreach (var row in values)
            {
                Debug.Log(string.Join(", ", row));
            }
        }
        else
        {
            Debug.Log("No data found.");
        }

        // Add the current time as a new row
        var newRow = new List<object>() { DateTime.Now.ToString() };
        var valueRange = new ValueRange();
        valueRange.Values = new List<IList<object>> { newRow };

        var appendRequest = service.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
        appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
        var appendResponse = appendRequest.Execute();

        Debug.Log("Added current time to the spreadsheet.");
    }
}