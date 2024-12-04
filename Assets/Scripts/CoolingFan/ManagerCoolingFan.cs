using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManagerCoolingFan : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public Button playButton;
    public Button pauseButton;
    public Button downloadButton;
    public TMP_InputField speedAccelInputField;
    public TMP_InputField promptInputField;

    // Subassemblies
    public RuntimeCoolingFan runtimeCoolingFan;
    public RuntimeFanSupport runtimeFanSupport;
    public RuntimeFanCrimping runtimeFanCrimping;

    private float timer = 0f;
    private bool isPlaying = false;
    void Start() { 
        playButton.onClick.AddListener(Play);
        pauseButton.onClick.AddListener(Pause);
        downloadButton.onClick.AddListener(Download);
    }

    // Update is called once per frame
    void Update() {
        if (isPlaying) {
            timer += Time.deltaTime * float.Parse(speedAccelInputField.text);
            timerText.text = timer.ToString("F2");
            runtimeCoolingFan.SetTime(timer);
            runtimeFanSupport.SetTime(timer);
            runtimeFanCrimping.SetTime(timer);
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
    void Download() {
        Debug.Log("Download");
    }


}
