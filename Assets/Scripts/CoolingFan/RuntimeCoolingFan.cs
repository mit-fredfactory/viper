using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class RuntimeCoolingFan : MonoBehaviour
{
    public float grabMaterialTime = 2f;
    public float screwTime = 8f;
    public float screwFailRate = 0.2f;

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
    
    public RuntimeFanSupport runtimeFanSupport;
    public RuntimeFanCrimping runtimeFanCrimping;
    public TextMeshProUGUI subassemblyCountText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI operatorEfficiencyText;

    public ManagerCoolingFan manager;

    // state machine
    private enum State {
        GrabCoolingSubassembly,
        GrabFanSubassembly,
        Screw,
        Done
    }

    private State currentState = State.GrabCoolingSubassembly;
    private float stateStartTime;
    private bool stateStarted;
    private int screwCount = 0;
    private int screwedFanCount = 0;
    private int subassemblyCount = 0;
    private int totalScrewAttempts = 0;
    private int totalSuccessfulScrews = 0;
    private float currentTime = 0;
    private float lastTime = 0;
    private float operatorEfficiency = 0;

    // Parameters for dataset of most recent elements
    public bool generateDataset = false;
    private bool instanceGenerated = false;
    private bool csvCreated = false;
    private string csvFilePath;
    private List<int> totalScrewAttemptsList = new List<int>();
    private List<int> totalSuccessfulScrewsList = new List<int>();

    public GameObject workingObject;
    Animator objectAnim;

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
            screwFailRate = 0.08f + (100 - operatorEfficiency) * 0.003f;

            switch (currentState) {
                case State.GrabCoolingSubassembly:
                    if (!stateStarted) {
                        if (runtimeFanSupport.GetSubassemblyCount() > 0) {
                            stateStartTime = currentTime;
                            stateStarted = true;
                            SetFeedbackText("Grabbing cooling subassembly");
                            runtimeFanSupport.SetSubassemblyCount(-1);
                            objectAnim = workingObject.GetComponent<Animator>();
                            workingObject.SetActive(false);
                            workingObject.SetActive(true);
                            objectAnim.SetFloat("GSMultiplier", manager.speed/grabMaterialTime);
                        } else {
                            SetFeedbackText("No cooling subassemblies available");
                        }
                    } else if (currentTime - stateStartTime > grabMaterialTime) {
                        currentState = State.GrabFanSubassembly;
                        stateStarted = false;
                    }
                    else
                    {
                        objectAnim.SetFloat("GSMultiplier", manager.speed/grabMaterialTime);
                    }
                    break;
                case State.GrabFanSubassembly:
                    if (!stateStarted) {
                        if (runtimeFanCrimping.GetSubassemblyCount() > 0) {
                            stateStartTime = currentTime;
                            stateStarted = true;
                            SetFeedbackText("Grabbing fan subassembly");
                            runtimeFanCrimping.SetSubassemblyCount(-1);
                            objectAnim.SetFloat("PFMultiplier", manager.speed/grabMaterialTime);
                        } else {
                            SetFeedbackText("No fan subassemblies available");
                        }
                    } else if (currentTime - stateStartTime > grabMaterialTime) {
                        currentState = State.Screw;
                        stateStarted = false;
                    }
                    else
                    {
                        objectAnim.SetFloat("PFMultiplier", manager.speed/grabMaterialTime);
                    }
                    break;
                case State.Screw:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                        SetFeedbackText("Screwing");
                    }
                    if (currentTime - stateStartTime > screwTime) {
                        if (Random.value < screwFailRate) {
                            SetFeedbackText("Screwing failed, retrying");
                        } else {
                            SetFeedbackText("Screw successful");
                            screwCount++;
                            totalSuccessfulScrews++;
                            if (screwCount == 2) {
                                SetFeedbackText("Fan screwed successfully");
                                screwedFanCount++;
                                if (screwedFanCount == 2) {
                                    SetFeedbackText("Both fans screwed successfully");
                                    currentState = State.Done;
                                    screwedFanCount = 0;
                                    objectAnim.SetTrigger("AllScrews");
                                } 
                                screwCount = 0;
                            }    
                        }
                        totalScrewAttempts++;
                        stateStarted = false;
                    }
                    break;
                case State.Done:
                    SetFeedbackText("Cooling Fan subassembly complete");
                    subassemblyCount++;
                    subassemblyCountText.text = subassemblyCount.ToString();
                    currentState = State.GrabCoolingSubassembly;
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
            objectAnim.SetFloat("GSMultiplier", 0f);
            objectAnim.SetFloat("PFMultiplier", 0f);
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
        totalScrewAttemptsList.Add(totalScrewAttempts);
        totalSuccessfulScrewsList.Add(totalSuccessfulScrews);
        int recentTotalScrewAttempts = totalScrewAttemptsList.Count > 10 ? 
            totalScrewAttemptsList.Last() -
            totalScrewAttemptsList[totalScrewAttemptsList.Count-10] 
            : totalScrewAttemptsList.Last();
        int recentTotalSuccessfulScrews = totalSuccessfulScrewsList.Count > 10 ?
            totalSuccessfulScrewsList.Last() - 
            totalSuccessfulScrewsList[totalSuccessfulScrewsList.Count-10] 
            : totalSuccessfulScrewsList.Last();
        if (recentTotalScrewAttempts == 0) {
            Debug.Log("No screw attempts in the last minute");
            return;
        }
        float successRate = (float)recentTotalSuccessfulScrews / recentTotalScrewAttempts;
        string instance = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}",
            currentTime, age, experienceInYears, trainingLevel, attentionLevel,
            cognitiveLoad, learningCurve, stressLevel, fatigueLevel, motivationLevel,
            ergonomicRating, noiseLevel, temperature, lighting, successRate);
        Debug.Log(instance);
        using (StreamWriter writer = new StreamWriter(csvFilePath, true)) {
            writer.WriteLine(instance);
        }
    } 
    private void CreateNewCSVFile() {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        csvFilePath = Path.Combine(Application.persistentDataPath, $"cooling_fan_{timestamp}.csv");
        using (StreamWriter writer = new StreamWriter(csvFilePath, false)) {
            writer.WriteLine("Time,Age,Experience,Training,Attention,Cognitive load,Learning curve,Stress,Fatigue,Motivation,Ergonomic rating,Noise,Temperature,Lighting,Success rate");
        }
        Debug.Log($"New CSV file created: {csvFilePath}");
    }
}
