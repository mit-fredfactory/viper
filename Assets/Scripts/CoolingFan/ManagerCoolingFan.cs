using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LLMUnity;

public class ManagerCoolingFan : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public Button playButton;
    public Button pauseButton;
    public Button downloadButton;
    public Button breakButton;
    public TextMeshProUGUI breakButtonText;
    public TMP_InputField speedAccelInputField;
    public TMP_InputField promptInputField;
    public TMP_InputField noiseLevelInputField;
    public TMP_InputField temperatureInputField;
    public TMP_InputField lightingInputField;
    public TMP_Text modelResponseText;
    public Button modelRequestButton;

    [Header("Subassemblies")]
    public RuntimeCoolingFan runtimeCoolingFan;
    public RuntimeFanSupport runtimeFanSupport;
    public RuntimeFanCrimping runtimeFanCrimping;

    [Header("LLM Configuration")]
    public bool useApiModel = false;
    public LLMCharacter localLlm;
    public ApiLlmInference apiLlm;

    private float timer = 0f;
    private bool isPlaying = false;
    private bool isOnBreak = false;
    public float speed;

    void Start() {
        playButton.onClick.AddListener(Play);
        pauseButton.onClick.AddListener(Pause);
        downloadButton.onClick.AddListener(Download);
        modelRequestButton.onClick.AddListener(RequestModel);
        
        if (breakButton != null) {
            breakButton.onClick.AddListener(ToggleBreak);
            UpdateBreakButtonText();
        }

        if (apiLlm != null)
        {
            apiLlm.OnChunkReceived += HandleApiReplyChunk;
            apiLlm.OnCompleteResponse += HandleApiCompleteResponse;
        } else if (useApiModel) {
             Debug.LogError("useApiModel is true, but no ApiLlmInference component is assigned!");
        }

        if (localLlm == null && !useApiModel) {
             Debug.LogError("useApiModel is false, but no local LLMCharacter component is assigned!");
        }
    }

    // Update is called once per frame
    void Update() {
        speed = float.Parse(speedAccelInputField.text);
        if (isPlaying) {
            timer += Time.deltaTime * speed;
            // set timer text to time in format: xx D xx H xx M xx S xx MS
            timerText.text = FormatTime(timer);
            SetTime(timer);

            // Update environmental factors
            SetEnvironmentalFactors(
                int.Parse(noiseLevelInputField.text),
                int.Parse(temperatureInputField.text),
                int.Parse(lightingInputField.text));
        }
    }

    void Play() {
        Debug.Log("Play");
        runtimeCoolingFan.SetRuntime(true);
        runtimeFanSupport.SetRuntime(true);
        runtimeFanCrimping.SetRuntime(true);
        isPlaying = true;
    }
    
    void Pause() {
        runtimeCoolingFan.SetRuntime(false);
        runtimeFanSupport.SetRuntime(false);
        runtimeFanCrimping.SetRuntime(false);
        isPlaying = false;
    }
    
    void ToggleBreak() {
        isOnBreak = !isOnBreak;
        UpdateBreakButtonText();
        
        // Set break status for all operators
        runtimeFanSupport.SetBreakStatus(isOnBreak);
        runtimeFanCrimping.SetBreakStatus(isOnBreak);
        runtimeCoolingFan.SetBreakStatus(isOnBreak);
    }
    
    void UpdateBreakButtonText() {
        if (breakButtonText != null) {
            breakButtonText.text = isOnBreak ? "End Break" : "Take Break";
        }
    }
    
    void Download() {
        Debug.Log("Download");
    }
    
    void RequestModel() {
        Debug.Log("Request Model");
        string userPrompt = promptInputField.text;
        string analysisData = "\n" + runtimeFanSupport.GetAnalysis() +
                              "\n" + runtimeFanCrimping.GetAnalysis() +
                              "\n" + runtimeCoolingFan.GetAnalysis();

        string fullMessage = userPrompt + analysisData;

        if (useApiModel)
        {
            if (apiLlm != null)
            {
                Debug.Log("Using API Model for request.");
                modelResponseText.text = "Sending request to API...";
                apiLlm.SendRequest(fullMessage);
            }
            else
            {
                Debug.LogError("API Model selected, but ApiLlmInference component is not assigned.");
                modelResponseText.text = "Error: API Handler not assigned.";
            }
        }
        else
        {
            if (localLlm != null)
            {
                Debug.Log("Using Local Model for request.");
                 modelResponseText.text = "Sending request to Local LLM...";
                localLlm.Chat(fullMessage, HandleLocalReply);
            }
            else
            {
                Debug.LogError("Local Model selected, but LLMCharacter component is not assigned.");
                 modelResponseText.text = "Error: Local LLM not assigned.";
            }
        }
    }
    
    void HandleLocalReply(string response) {
        Debug.Log("Local LLM Reply Received.");
        modelResponseText.text = response;
    }

    void HandleApiReplyChunk(string chunk)
    {
        if (modelResponseText.text == "Sending request to API...")
        {
            modelResponseText.text = chunk;
        }
        else
        {
            modelResponseText.text += chunk;
        }
    }

    void HandleApiCompleteResponse(string finalResponse)
    {
         Debug.Log("API LLM Full Response Received.");
        if (finalResponse.StartsWith("Error:")) {
             modelResponseText.text = finalResponse;
        }
    }

    public static string FormatTime(float time) {
        int days = (int)(time / 86400);
        time -= days * 86400;
        int hours = (int)(time / 3600);
        time -= hours * 3600;
        int minutes = (int)(time / 60);
        time -= minutes * 60;
        int seconds = (int)time;

        return string.Format("{0:00} D    {1:00} H    {2:00} M    {3:00} S", days, hours, minutes, seconds);
    }

    public void SetTime(float time) {
        runtimeCoolingFan.SetTime(timer);
        runtimeFanSupport.SetTime(timer);
        runtimeFanCrimping.SetTime(timer);
    }
    
    public void SetEnvironmentalFactors(int noiseLevel, int temperature, int lighting) {
        runtimeCoolingFan.SetEnvironmentalFactors(noiseLevel, temperature, lighting);
        runtimeFanSupport.SetEnvironmentalFactors(noiseLevel, temperature, lighting);
        runtimeFanCrimping.SetEnvironmentalFactors(noiseLevel, temperature, lighting);
    }
}
