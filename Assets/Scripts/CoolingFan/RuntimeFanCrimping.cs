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
    public OperatorParams operatorParams;
    [Header("Environmental factors")]
    public int noiseLevel;
    public int temperature;
    public int lighting;
    
    [Header("Process factors")]
    public int ergonomicRating = 7;
    
    public bool runtimeStarted = false;
    public bool onBreak = false;
    
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
    private float crimpWireStartTime;
    private bool crimpWireStarted;
    private int crimpedFanCount = 0;
    private int subassemblyCount = 0;
    private int failedSubassemblyCount = 0;
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
        // Initialize operator parameters if not already set
        if (operatorParams == null)
        {
            operatorParams = new OperatorParams();
        }
    }

    void Update() {
        if (runtimeStarted) {
            float deltaTime = currentTime - lastTime;
            lastTime = currentTime;
            
            // Update operator parameters based on whether they're on break or working
            if (onBreak) {
                operatorParams.UpdateRestingParams(deltaTime);
                
                // While on break, animate the operator resting if possible
                SetFeedbackText("Operator on break");
            } 
            else {
                // Update operator parameters for working state
                float multiplier = 0.0001f * deltaTime;
                operatorParams.UpdateWorkingParams(deltaTime, multiplier);
                
                // Calculate operator efficiency
                operatorEfficiency = operatorParams.CalculateEfficiency(noiseLevel, temperature, lighting, ergonomicRating);
                operatorEfficiencyText.text = operatorEfficiency.ToString();
                crimpWireFailRate = 0.08f + (100 - operatorEfficiency) * 0.003f;
                
                // Only process the state machine if not on break
                ProcessStateMachine();
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

    private void ProcessStateMachine() {
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
                        failedSubassemblyCount++;
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
    }

    public void SetBreakStatus(bool isOnBreak) {
        onBreak = isOnBreak;
        if (isOnBreak) {
            SetFeedbackText("Operator taking a break");
            // Pause any ongoing work similar to RuntimeFanSupport
        } else {
            SetFeedbackText("Operator resumed work");
            // Resume animation if needed
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
        // show successful divided by failed in format "successful/int(failed/2)"
        int dividedFailedSubassemblyCount = failedSubassemblyCount / 2;
        subassemblyCountText.text = $"{subassemblyCount}/{dividedFailedSubassemblyCount}";
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
            currentTime, operatorParams.age, operatorParams.experienceInYears, operatorParams.trainingLevel, operatorParams.attentionLevel,
            operatorParams.cognitiveLoad, operatorParams.learningCurve, operatorParams.stressLevel, operatorParams.fatigueLevel, operatorParams.motivationLevel,
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
