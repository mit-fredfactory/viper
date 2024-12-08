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
    public TMP_InputField noiseLevelInputField;
    public TMP_InputField temperatureInputField;
    public TMP_InputField lightingInputField;

    // Subassemblies
    public RuntimeCoolingFan runtimeCoolingFan;
    public RuntimeFanSupport runtimeFanSupport;
    public RuntimeFanCrimping runtimeFanCrimping;

    private float timer = 0f;
    private bool isPlaying = false;
    public float speed;
    void Start() { 
        playButton.onClick.AddListener(Play);
        pauseButton.onClick.AddListener(Pause);
        downloadButton.onClick.AddListener(Download);
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
    void Download() {
        Debug.Log("Download");
    }
    public static string FormatTime(float time) {
        int days = (int)(time / 86400);
        time -= days * 86400;
        int hours = (int)(time / 3600);
        time -= hours * 3600;
        int minutes = (int)(time / 60);
        time -= minutes * 60;
        int seconds = (int)time;
        int milliseconds = (int)((time - seconds) * 1000);

        return string.Format("{0:00} D\n{1:00} H\n{2:00} M\n{3:00} S\n{4:000} MS", days, hours, minutes, seconds, milliseconds);
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
