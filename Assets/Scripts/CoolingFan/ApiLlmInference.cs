using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;
using TMPro; // Assuming you might want to display responses or status

public class ApiLlmInference : MonoBehaviour
{
    [Header("API Configuration")]
    public string apiUrl = "https://openrouter.ai/api/v1/chat/completions"; // Example URL
    public string apiKey = "<YOUR_API_KEY>"; // Remember to replace this or load securely
    public string modelName = "openai/gpt-4o"; // Example model

    [Header("Prompts")]
    [TextArea(5, 10)]
    public string systemPrompt = "You are a helpful assistant.";

    [Header("Output")]
    public TMP_Text responseText; // Optional: Assign a TextMeshPro UI element to display the full response

    public Action<string> OnChunkReceived; // Event for streaming chunks
    public Action<string> OnCompleteResponse; // Event for the complete response

    private Coroutine runningRequest = null;

    // Public method to initiate the request
    public void SendRequest(string userPrompt)
    {
        if (runningRequest != null)
        {
            Debug.LogWarning("API request already in progress. Stopping the previous one.");
            StopCoroutine(runningRequest);
        }
        Debug.Log($"Sending API request to {apiUrl} with model {modelName}");
        runningRequest = StartCoroutine(StreamRequestCoroutine(userPrompt));
    }

    private IEnumerator StreamRequestCoroutine(string userPrompt)
    {
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            // --- Request Body ---
            string jsonPayload = $@"{{
                ""model"": ""{modelName}"",
                ""messages"": [
                    {{""role"": ""system"", ""content"": ""{EscapeJsonString(systemPrompt)}""}},
                    {{""role"": ""user"", ""content"": ""{EscapeJsonString(userPrompt)}""}}
                ],
                ""stream"": true
            }}";
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);

            // --- Download Handler (Custom for Streaming) ---
            request.downloadHandler = new DownloadHandlerBuffer(); // Use buffer initially, process later

            // --- Headers ---
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            // --- Send Request ---
            var asyncOperation = request.SendWebRequest();
            Debug.Log("API Request sent. Waiting for response...");

            float lastReceivedTime = Time.realtimeSinceStartup;
            StringBuilder currentChunkBuffer = new StringBuilder();
            string fullResponse = "";
            ulong processedBytesCount = 0; // Keep track of processed bytes

            while (!asyncOperation.isDone)
            {
                ulong currentDownloadedBytes = request.downloadedBytes; // Check current downloaded bytes
                if (currentDownloadedBytes > processedBytesCount)
                {
                    // Get only the new data received since the last check
                    string receivedData = ((DownloadHandlerBuffer)request.downloadHandler).text;
                    string newData = receivedData.Substring((int)processedBytesCount); // Cast ulong to int, assuming chunks aren't > 2GB

                    // No longer needed: Clear the buffer as we process it
                    // request.downloadHandler = new DownloadHandlerBuffer(); // <--- REMOVE THIS LINE

                    // Process received data (similar to Python example)
                    currentChunkBuffer.Append(newData); // Append only the new data
                    string bufferContent = currentChunkBuffer.ToString();
                    int lineEndIndex;

                    while ((lineEndIndex = bufferContent.IndexOf('\n')) != -1)
                    {
                        string line = bufferContent.Substring(0, lineEndIndex).Trim();
                        bufferContent = bufferContent.Substring(lineEndIndex + 1); // Remaining part

                        if (line.StartsWith("data: "))
                        {
                            string jsonData = line.Substring(6);
                            if (jsonData != "[DONE]")
                            {
                                try
                                {
                                    // Very basic JSON parsing - consider using a library for robustness
                                    if (jsonData.Contains("\"content\":"))
                                    {
                                        int contentStart = jsonData.IndexOf("\"content\":\"") + 11;
                                        int contentEnd = jsonData.IndexOf("\"", contentStart);
                                        if (contentStart > 10 && contentEnd > contentStart)
                                        {
                                            string content = jsonData.Substring(contentStart, contentEnd - contentStart);
                                            // Unescape common JSON escape sequences if necessary
                                            content = content.Replace("\\n", "\n").Replace("\\t", "\t").Replace("\\\"", "\"");
                                            Debug.Log($"Received chunk: {content}");
                                            OnChunkReceived?.Invoke(content);
                                            fullResponse += content;
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError($"Error parsing JSON chunk: {jsonData}\nError: {e.Message}");
                                }
                            }
                        }
                    }
                    // Put unprocessed part back to buffer
                    currentChunkBuffer.Clear();
                    currentChunkBuffer.Append(bufferContent);

                    processedBytesCount = currentDownloadedBytes; // Update the processed byte count
                    lastReceivedTime = Time.realtimeSinceStartup;
                } else if (Time.realtimeSinceStartup - lastReceivedTime > 10f) // Timeout check
                {
                     Debug.LogWarning("API request timeout (10s).");
                     // Handle timeout - maybe retry or abort
                     break;
                }

                yield return null; // Wait for the next frame
            }

            // --- Handle Final Result ---
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"API Error: {request.error} - Response Code: {request.responseCode}");
                // Check if downloadHandler is not null before accessing its text property
                if(request.downloadHandler != null && request.downloadHandler.text != null)
                {
                    Debug.LogError($"Error Details: {request.downloadHandler.text}");
                }
                 OnCompleteResponse?.Invoke($"Error: {request.error}");
            }
            else if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("API Request completed successfully.");
                // Process any remaining data in the buffer
                 string remainingData = currentChunkBuffer.ToString();
                 // (Add similar processing logic as in the loop if needed for the last part)

                 OnCompleteResponse?.Invoke(fullResponse);
                 if (responseText != null) responseText.text = fullResponse;
            }
             else
             {
                 Debug.LogWarning($"API Request finished with result: {request.result}");
                  OnCompleteResponse?.Invoke($"Finished with: {request.result}");
             }
        }
        runningRequest = null; // Mark as finished
    }

     // Helper to escape strings for JSON payload
    private string EscapeJsonString(string str)
    {
        if (string.IsNullOrEmpty(str)) return "";
        return str.Replace("\\", "\\\\") // Escape backslashes first
                  .Replace("\"", "\\\"") // Escape quotes
                  .Replace("\n", "\\n")  // Escape newlines
                  .Replace("\r", "\\r")  // Escape carriage returns
                  .Replace("\t", "\\t"); // Escape tabs
    }

    void OnDestroy()
    {
        // Ensure the coroutine is stopped if the object is destroyed
        if (runningRequest != null)
        {
            StopCoroutine(runningRequest);
        }
    }
} 