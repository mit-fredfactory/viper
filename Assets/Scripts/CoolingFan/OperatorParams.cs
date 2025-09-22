using UnityEngine;
using System;

[System.Serializable]
public class OperatorParams : MonoBehaviour {
    [Header("Demographic information")]
    public int age;
    public int experienceInYears;
    public int trainingLevel;
    
    [Header("Cognitive factors")]
    public float attentionLevel;
    public float cognitiveLoad;
    public float learningCurve;
    
    [Header("Physiological factors")]
    public float stressLevel;
    public float fatigueLevel;
    public float motivationLevel;

    // Rates for recovery during breaks
    public float attentionRecoveryRate = 0.001f;
    public float stressRecoveryRate = 0.0008f;
    public float fatigueRecoveryRate = 0.0005f;
    public float motivationRecoveryRate = 0.0006f;
    public float cognitiveLoadRecoveryRate = 0.0007f;
    
    // Set default values for a new operator
    void Start() {
        /*age = 35;
        experienceInYears = 5;
        trainingLevel = 5;
        attentionLevel = 10;
        cognitiveLoad = 0;
        learningCurve = 5;
        ergonomicRating = 7;
        stressLevel = 0;
        fatigueLevel = 0;
        motivationLevel = 10;*/
    }
    
    // Update operator parameters during active work
    public void UpdateWorkingParams(float deltaTime, float multiplier)
    {
        attentionLevel = Math.Clamp(attentionLevel - multiplier, 0, 10);
        cognitiveLoad = Math.Clamp(cognitiveLoad + multiplier, 0, 10);
        learningCurve = Math.Clamp(learningCurve - multiplier * 5, 0, 10);
        stressLevel = Math.Clamp(stressLevel + multiplier, 0, 10);
        fatigueLevel = Math.Clamp(fatigueLevel + multiplier, 0, 10);
        motivationLevel = Math.Clamp(motivationLevel - multiplier, 0, 10);
    }
    
    // Update operator parameters during rest/breaks
    public void UpdateRestingParams(float deltaTime)
    {
        float restMultiplier = deltaTime;
        attentionLevel = Math.Clamp(attentionLevel + attentionRecoveryRate * restMultiplier, 0, 10);
        cognitiveLoad = Math.Clamp(cognitiveLoad - cognitiveLoadRecoveryRate * restMultiplier, 0, 10);
        stressLevel = Math.Clamp(stressLevel - stressRecoveryRate * restMultiplier, 0, 10);
        fatigueLevel = Math.Clamp(fatigueLevel - fatigueRecoveryRate * restMultiplier, 0, 10);
        motivationLevel = Math.Clamp(motivationLevel + motivationRecoveryRate * restMultiplier, 0, 10);
    }
    
    // Calculate operator efficiency based on all parameters
    public float CalculateEfficiency(int noiseLevel, int temperature, int lighting, int ergonomicRating)
    {
        float efficiency = 60 + 1 * (35 - age) + 1 * experienceInYears + 2 * trainingLevel +
            1 * attentionLevel - 1 * cognitiveLoad - 1 * learningCurve - 1 * stressLevel -
            1 * fatigueLevel + 1 * motivationLevel + 1 * ergonomicRating +
            1 * (70 - noiseLevel) -
            1 * Math.Abs(20 - temperature) - 0.2f * Math.Abs(70 - lighting);
            
        return Mathf.Clamp(efficiency, 0, 100);
    }
} 