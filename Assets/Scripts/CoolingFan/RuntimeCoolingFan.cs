using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using TMPro;
using System;

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
    private int screwCount= 0;
    private int screwedFanCount = 0;
    private int subassemblyCount = 0;
    private float currentTime = 0;
    private float lastTime = 0;
    private float operatorEfficiency = 0;

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
                        } else {
                            SetFeedbackText("No cooling subassemblies available");
                        }
                    } else if (currentTime - stateStartTime > grabMaterialTime) {
                        currentState = State.GrabFanSubassembly;
                        stateStarted = false;
                    }
                    break;
                case State.GrabFanSubassembly:
                    if (!stateStarted) {
                        if (runtimeFanCrimping.GetSubassemblyCount() > 0) {
                            stateStartTime = currentTime;
                            stateStarted = true;
                            SetFeedbackText("Grabbing fan subassembly");
                            runtimeFanCrimping.SetSubassemblyCount(-1);
                        } else {
                            SetFeedbackText("No fan subassemblies available");
                        }
                    } else if (currentTime - stateStartTime > grabMaterialTime) {
                        currentState = State.Screw;
                        stateStarted = false;
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
                            if (screwCount == 2) {
                                SetFeedbackText("Fan screwed successfully");
                                screwedFanCount++;
                                if (screwedFanCount == 2) {
                                    SetFeedbackText("Both fans screwed successfully");
                                    currentState = State.Done;
                                    screwedFanCount = 0;
                                } 
                                screwCount = 0;
                            }    
                        }
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
}
