using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class RuntimeFanSupport : MonoBehaviour
{
    public float grabMaterialTime = 2f;
    public float positionFanSupportOnJigTime = 4f;
    public float addHeatInsertTime = 6f;
    public float heatInsertFailureRate = 0.1f;
    public float moveSubassemblyTime = 5f;
    // UI Label for operator parameters
    [Header("Operator Parameters")]
    [Header("Demographic information")]
    public int age;
    public int experienceInYears;
    public int trainingLevel;
    [Header("Cognitive factors")]
    public float attentionLevel;
    public float cognitiveLoad;
    public float learningCurve;
    [Header("Physiological factors")]
    public float ergonomicRating;
    public float stressLevel;
    public float fatigueLevel;
    public float motivationLevel;
    [Header("Environmental factors")]
    public int noiseLevel;
    public int temperature;
    public int lighting;

    public bool runtimeStarted = false;
    
    public TextMeshProUGUI subassemblyCountText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI operatorEfficiencyText;

    public ManagerCoolingFan manager;

    // state machine
    private enum State {
        GrabFanSupport,
        PositionFanSupportOnJig,
        M5Insert,
        M3Insert,
        MoveSubassembly,
        Done
    }

    private State currentState = State.GrabFanSupport;
    private float stateStartTime;
    private bool stateStarted;
    private float heatInsertStartTime;
    private bool heatInsertStarted;

    private int m5InsertCount = 0;
    private int m3InsertCount = 0;
    private int subassemblyCount = 0;
    private int totalInsertAttempts = 0;
    private int totalSuccessfulInserts = 0;
    private float currentTime = 0;
    private float lastTime = 0;
    private float operatorEfficiency = 0;
    
    // Parameters for dataset of most recent elements
    public bool generateDataset = false;
    private bool instanceGenerated = false;
    private bool csvCreated = false;
    private string csvFilePath;
    private List<int> totalInsertAttemptsList = new List<int>();
    private List<int> totalSuccessfulInsertsList = new List<int>();

    public GameObject workingObject;
    Animator objectAnim;
    public List<GameObject> heatInserts = new List<GameObject>();
    public Animator operatorAnim;
    private string analysisString;
    private int analysisCount = 0;

    void Start(){
        workingObject.SetActive(false);
    }

    void Update() {
        if (runtimeStarted) {
            float deltaTime = currentTime - lastTime;
            lastTime = currentTime;
            // Determine operator efficiency based on operator parameters maximum
            // efficiency is 100% and minimum efficiency is 0%
            float multiplier = 0.0001f * deltaTime;
            attentionLevel = Math.Clamp(attentionLevel-multiplier, 0, 10);
            cognitiveLoad = Math.Clamp(cognitiveLoad+multiplier, 0, 10);
            learningCurve = Math.Clamp(learningCurve-multiplier*5, 0, 10);
            stressLevel = Math.Clamp(stressLevel+multiplier, 0, 10);
            fatigueLevel = Math.Clamp(fatigueLevel+multiplier, 0, 10);
            motivationLevel = Math.Clamp(motivationLevel-multiplier, 0, 10);

            operatorEfficiency = 60 + 1 * (35-age) + 1 * experienceInYears + 2 * trainingLevel +
                1 * attentionLevel - 1 * cognitiveLoad - 1 * learningCurve - 1 * stressLevel -
                1 * fatigueLevel + 1 * motivationLevel + 1 * ergonomicRating +
                1 * (70-noiseLevel) -
                1 * Math.Abs(20-temperature) - 0.2f * Math.Abs(70-lighting);
            //Debug.Log("Operator efficiency: " + operatorEfficiency);
            operatorEfficiency = Mathf.Clamp(operatorEfficiency, 0, 100);
            operatorEfficiencyText.text = operatorEfficiency.ToString();
            heatInsertFailureRate = 0.002f + (100 - operatorEfficiency) * 0.003f;
            
            switch (currentState) {
                case State.GrabFanSupport:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                        m5InsertCount = 0;
                        m3InsertCount = 0;
                        objectAnim = workingObject.GetComponent<Animator>();
                        workingObject.SetActive(true);
                        objectAnim.SetFloat("GFSMultiplier", manager.speed/grabMaterialTime);
                    }
                    if (currentTime - stateStartTime > grabMaterialTime) {
                        currentState = State.PositionFanSupportOnJig;
                        stateStarted = false;
                        SetFeedbackText("Fan support grabbed, positioning on jig");
                    }
                    else
                    {
                        objectAnim.SetFloat("GFSMultiplier", manager.speed/grabMaterialTime);
                    }
                    break;
                case State.PositionFanSupportOnJig:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                        operatorAnim.SetBool("Grab", true);
                    }
                    if (currentTime - stateStartTime >
                        positionFanSupportOnJigTime) {
                        currentState = State.M5Insert;
                        stateStarted = false;
                        SetFeedbackText("Fan support positioned, inserting M5");
                        operatorAnim.SetBool("Grab", false);
                    }
                    break;
                case State.M5Insert:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                        operatorAnim.SetBool("Grab", true);
                    }
                    if (stateStarted && currentTime - stateStartTime > 
                        grabMaterialTime) {
                        if (!heatInsertStarted) {
                            heatInsertStartTime = currentTime;
                            heatInsertStarted = true;
                            SetFeedbackText("Inserting M5 insert");
                            operatorAnim.SetBool("Grab", false);
                            operatorAnim.SetBool("Screw", true);
                        }
                        if (currentTime - heatInsertStartTime > addHeatInsertTime) {
                            if (Random.value < heatInsertFailureRate) {
                                SetFeedbackText("M5 insert failed, subassembly to scrap");
                                currentState = State.GrabFanSupport;
                            } else {
                                totalSuccessfulInserts++;
                                SetFeedbackText("M5 insert successful");
                                heatInserts[m5InsertCount].SetActive(true);
                                m5InsertCount++;
                                if (m5InsertCount == 2) {
                                    currentState = State.M3Insert;
                                    SetFeedbackText("M5 inserts successful, inserting M3 inserts");
                                }
                            }
                            totalInsertAttempts++;
                            stateStarted = false;
                            heatInsertStarted = false;
                            operatorAnim.SetBool("Screw", false);
                        }
                    }
                    break;
                case State.M3Insert:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                        operatorAnim.SetBool("Grab", true);
                    }
                    if (stateStarted && currentTime - stateStartTime > 
                        grabMaterialTime) {
                        if (!heatInsertStarted) {
                            heatInsertStartTime = currentTime;
                            heatInsertStarted = true;
                            SetFeedbackText("Inserting M3 insert");
                            operatorAnim.SetBool("Grab", false);
                            operatorAnim.SetBool("Screw", true);
                        }
                        if (currentTime - heatInsertStartTime > addHeatInsertTime) {
                            if (Random.value < heatInsertFailureRate) {
                                SetFeedbackText("M3 insert failed, subassembly to scrap");
                                currentState = State.GrabFanSupport;
                            } else {
                                SetFeedbackText("M3 insert successful");
                                heatInserts[2+m3InsertCount].SetActive(true);
                                m3InsertCount++;
                                if (m3InsertCount == 4) {
                                    currentState = State.MoveSubassembly;
                                    SetFeedbackText("M3 inserts successful, moving subassembly");
                                }
                            }
                            stateStarted = false;
                            heatInsertStarted = false;
                            operatorAnim.SetBool("Screw", false);
                        }
                    }
                    break;
                case State.MoveSubassembly:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                    }
                    if (currentTime - stateStartTime > moveSubassemblyTime) {
                        SetFeedbackText("Cooling subassembly complete");
                        currentState = State.Done;
                        stateStarted = false;
                    }
                    break;
                case State.Done:
                    workingObject.SetActive(false);
                    for (int i = 0; i < heatInserts.Count; i++) {
                        heatInserts[i].SetActive(false);
                    }
                    SetSubassemblyCount(1);
                    currentState = State.GrabFanSupport;
                    break;
            }

            // Generate dataset
            if (generateDataset && (int)currentTime % 60 == 0) {
                if (!instanceGenerated) {
                    AddDataInstance();
                    instanceGenerated = true;
                }
            } else {
                instanceGenerated = false;
                
            }
        }
        else if(objectAnim != null)
        {
            objectAnim.SetFloat("GFSMultiplier", 0f);
        }
    }
    
    public void SetRuntime(bool start) {
        runtimeStarted = start;
    }
    public void SetTime(float time) {
        currentTime = time;
    }
    public int GetSubassemblyCount() {
        return subassemblyCount;
    }
    public void SetSubassemblyCount(int sign) {
        subassemblyCount += sign;
        subassemblyCountText.text = subassemblyCount.ToString();
    }
    public void SetFeedbackText(string text) {
        feedbackText.text = text;
    }
    public void SetEnvironmentalFactors(int noise, int temp, int light) {
        noiseLevel = noise;
        temperature = temp;
        lighting = light;
    }
    public void AddDataInstance() {
        if (!csvCreated) {
            CreateNewCSVFile();
            csvCreated = true;
        }
        // if list index is higher than 10, substract the element 10 steps ago from the total
        totalInsertAttemptsList.Add(totalInsertAttempts);
        totalSuccessfulInsertsList.Add(totalSuccessfulInserts);
        int recentTotalInsertAttempts = totalInsertAttemptsList.Count > 10 ? 
            totalInsertAttemptsList.Last() - 
            totalInsertAttemptsList[totalInsertAttemptsList.Count-10] 
            : totalInsertAttemptsList.Last();
        int recentTotalSuccessfulInserts = totalSuccessfulInsertsList.Count > 10 ?
            totalSuccessfulInsertsList.Last() - 
            totalSuccessfulInsertsList[totalSuccessfulInsertsList.Count-10] 
            : totalSuccessfulInsertsList.Last();
        if (recentTotalInsertAttempts == 0) {
            Debug.Log("No screw attempts in the last minute");
            return;
        }
        float successRate = (float)recentTotalSuccessfulInserts / recentTotalInsertAttempts;
        string instance = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}",
            currentTime, age, experienceInYears, trainingLevel, attentionLevel,
            cognitiveLoad, learningCurve, stressLevel, fatigueLevel, motivationLevel,
            ergonomicRating, noiseLevel, temperature, lighting, successRate);
        Debug.Log(instance);
        using (StreamWriter writer = new StreamWriter(csvFilePath, true)) {
            writer.WriteLine(instance);
            analysisCount++;
            if (analysisCount % 10 == 0) {
                analysisString += instance + "\n";
            }
        }
    } 
    private void CreateNewCSVFile() {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        csvFilePath = Path.Combine(Application.persistentDataPath, $"fan_support_{timestamp}.csv");
        using (StreamWriter writer = new StreamWriter(csvFilePath, false)) {
            writer.WriteLine("Time,Age,Experience,Training,Attention,Cognitive load,Learning curve,Stress,Fatigue,Motivation,Ergonomic rating,Noise,Temperature,Lighting,Success rate");
            analysisString = "Cooling subassembly:\nTime,Age,Experience,Training,Attention,Cognitive load,Learning curve,Stress,Fatigue,Motivation,Ergonomic rating,Noise,Temperature,Lighting,Success rate\n";
        }
        Debug.Log($"New CSV file created: {csvFilePath}");
    }

    public string GetAnalysis() {
        return analysisString;
    }
}
