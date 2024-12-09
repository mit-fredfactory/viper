using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class RuntimeFanCrimping : MonoBehaviour
{
    public float grabMaterialTime = 2f;
    public float crimpWireTime = 8f;
    public float crimpWireFailRate = 0.4f;
    public float moveSubassemblyTime = 5f;

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

    public GameObject[] workingObject;
    public Animator[] objectAnim;

    // state machine
    private enum State {
        GrabFan,
        GrabWire,
        CrimpWire,
        MoveSubassembly,
        Done
    }

    private State currentState = State.GrabFan;
    private float stateStartTime;
    private bool stateStarted;
    private int crimpedFanCount = 0;
    private int subassemblyCount = 0;
    private int totalCrimpAttempts = 0;
    private int totalSuccessfulCrimps = 0;
    private float currentTime = 0;
    private float lastTime = 0;
    private float operatorEfficiency = 0;
    // Parameters for dataset of most recent elements
    public bool generateDataset = false;
    private bool instanceGenerated = false;
    private bool csvCreated = false;
    private string csvFilePath;
    private List<int> totalCrimpAttemptsList = new List<int>();
    private List<int> totalSuccessfulCrimpsList = new List<int>();
    public Animator operatorAnim;
    private string analysisString;
    private int analysisCount = 0;

    void Start(){
        workingObject[0].SetActive(false);
        workingObject[1].SetActive(false);
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
            crimpWireFailRate = 0.08f + (100 - operatorEfficiency) * 0.003f;

            switch (currentState) {
                case State.GrabFan:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                        workingObject[crimpedFanCount].SetActive(true);
                        objectAnim[crimpedFanCount].SetFloat("GFMultiplier", manager.speed/grabMaterialTime);
                    }
                    if (currentTime - stateStartTime > grabMaterialTime) {
                        currentState = State.GrabWire;
                        stateStarted = false;
                        SetFeedbackText("Fan in workstation, grabbing wire");
                    } else {
                        objectAnim[crimpedFanCount].SetFloat("GFMultiplier", manager.speed/grabMaterialTime);
                    }
                    break;
                case State.GrabWire:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                        objectAnim[crimpedFanCount].SetFloat("GWMultiplier", manager.speed/grabMaterialTime);
                        operatorAnim.SetBool("Grab", true);
                    }
                    if (currentTime - stateStartTime > grabMaterialTime) {
                        currentState = State.CrimpWire;
                        stateStarted = false;
                        SetFeedbackText("Crimping wire");
                        operatorAnim.SetBool("Grab", false);
                    }
                    else
                    {
                        objectAnim[crimpedFanCount].SetFloat("GWMultiplier", manager.speed/grabMaterialTime);
                    }
                    break;
                case State.CrimpWire:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                        operatorAnim.SetBool("Screw", true);
                    }
                    if (currentTime - stateStartTime > crimpWireTime) {
                        if (Random.value < crimpWireFailRate) {
                            SetFeedbackText("Wire crimping failed, retrying with new fan");
                            currentState = State.GrabFan;
                            workingObject[crimpedFanCount].SetActive(false);
                        } else {
                            totalSuccessfulCrimps++;
                            crimpedFanCount++;
                            if (crimpedFanCount == 2) {
                                SetFeedbackText("Wire crimping successful, moving subassembly");
                                currentState = State.MoveSubassembly;
                                crimpedFanCount = 0;
                            } else {
                                SetFeedbackText("Wire crimping successful, grabbing new fan");
                                currentState = State.GrabFan;
                            } 
                        }
                        totalCrimpAttempts++;
                        stateStarted = false;
                        operatorAnim.SetBool("Screw", false);
                    }
                    break;
                case State.MoveSubassembly:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                    }
                    if (currentTime - stateStartTime > moveSubassemblyTime) {
                        currentState = State.Done;
                        stateStarted = false;
                    }
                    break;
                case State.Done:
                    SetFeedbackText("Fan subassembly complete, grabbing fan");
                    subassemblyCount++;
                    subassemblyCountText.text = subassemblyCount.ToString();
                    currentState = State.GrabFan;
                    workingObject[0].SetActive(false);
                    workingObject[1].SetActive(false);
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
        else if(objectAnim[crimpedFanCount] != null)
        {
            objectAnim[crimpedFanCount].SetFloat("GFMultiplier", 0f);
            objectAnim[crimpedFanCount].SetFloat("GWMultiplier", 0f);
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
        totalCrimpAttemptsList.Add(totalCrimpAttempts);
        totalSuccessfulCrimpsList.Add(totalSuccessfulCrimps);
        int recentTotalCrimpAttempts = totalCrimpAttemptsList.Count > 10 ? 
            totalCrimpAttemptsList.Last() - 
            totalCrimpAttemptsList[totalCrimpAttemptsList.Count-10] 
            : totalCrimpAttemptsList.Last();
        int recentTotalSuccessfulCrimps = totalSuccessfulCrimpsList.Count > 10 ?
            totalSuccessfulCrimpsList.Last() -
            totalSuccessfulCrimpsList[totalSuccessfulCrimpsList.Count-10]
            : totalSuccessfulCrimpsList.Last();
        if (recentTotalCrimpAttempts == 0) {
            Debug.Log("No screw attempts in the last minute");
            return;
        }
        float successRate = (float)recentTotalSuccessfulCrimps / recentTotalCrimpAttempts;
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
        csvFilePath = Path.Combine(Application.persistentDataPath, $"fan_crimping_{timestamp}.csv");
        using (StreamWriter writer = new StreamWriter(csvFilePath, false)) {
            writer.WriteLine("Time,Age,Experience,Training,Attention,Cognitive load,Learning curve,Stress,Fatigue,Motivation,Ergonomic rating,Noise,Temperature,Lighting,Success rate");
            analysisString = "Fan subassembly:\nTime,Age,Experience,Training,Attention,Cognitive load,Learning curve,Stress,Fatigue,Motivation,Ergonomic rating,Noise,Temperature,Lighting,Success rate\n";
        }
        Debug.Log($"New CSV file created: {csvFilePath}");
    }

    public string GetAnalysis() {
        return analysisString;
    }
}
