using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using TMPro;

public class RuntimeFanSupport : MonoBehaviour
{
    public float grabMaterialTime = 2f;
    public float positionFanSupportOnJigTime = 4f;
    public float addHeatInsertTime = 6f;
    public float heatInsertFailureRate = 0.1f;
    public float moveSubassemblyTime = 5f;

    public bool runtimeStarted = false;
    
    public TextMeshProUGUI subassemblyCountText;
    public TextMeshProUGUI feedbackText;

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
    private float currentTime = 0;

    void Update() {
        if (runtimeStarted) {
            switch (currentState) {
                case State.GrabFanSupport:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                        m5InsertCount = 0;
                        m3InsertCount = 0;
                        SetFeedbackText("Grabbing fan support");
                    }
                    if (currentTime - stateStartTime > grabMaterialTime) {
                        currentState = State.PositionFanSupportOnJig;
                        stateStarted = false;
                    }
                    break;
                case State.PositionFanSupportOnJig:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                        SetFeedbackText("Positioning fan support on jig");
                    }
                    if (currentTime - stateStartTime >
                        positionFanSupportOnJigTime) {
                        currentState = State.M5Insert;
                        stateStarted = false;
                    }
                    break;
                case State.M5Insert:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                        SetFeedbackText("Grabbing M5 insert");
                    }
                    if (stateStarted && currentTime - stateStartTime > 
                        grabMaterialTime) {
                        if (!heatInsertStarted) {
                            heatInsertStartTime = currentTime;
                            heatInsertStarted = true;
                            SetFeedbackText("Inserting M5 insert");
                        }
                        if (currentTime - heatInsertStartTime > addHeatInsertTime) {
                            if (Random.value < heatInsertFailureRate) {
                                SetFeedbackText("M5 insert failed, subassembly to scrap");
                                currentState = State.GrabFanSupport;
                            } else {
                                SetFeedbackText("M5 insert successful");
                                m5InsertCount++;
                                if (m5InsertCount == 2)
                                    currentState = State.M3Insert;
                            }
                            stateStarted = false;
                            heatInsertStarted = false;
                        }
                    }
                    break;
                case State.M3Insert:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                        SetFeedbackText("Grabbing M3 insert");
                    }
                    if (stateStarted && currentTime - stateStartTime > 
                        grabMaterialTime) {
                        if (!heatInsertStarted) {
                            heatInsertStartTime = currentTime;
                            heatInsertStarted = true;
                            SetFeedbackText("Inserting M3 insert");
                        }
                        if (currentTime - heatInsertStartTime > addHeatInsertTime) {
                            if (Random.value < heatInsertFailureRate) {
                                SetFeedbackText("M3 insert failed, subassembly to scrap");
                                currentState = State.GrabFanSupport;
                            } else {
                                SetFeedbackText("M3 insert successful");
                                m3InsertCount++;
                                if (m3InsertCount == 4)
                                    currentState = State.MoveSubassembly;
                            }
                            stateStarted = false;
                            heatInsertStarted = false;
                        }
                    }
                    break;
                case State.MoveSubassembly:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                        SetFeedbackText("Moving subassembly");
                    }
                    if (currentTime - stateStartTime > moveSubassemblyTime) {
                        currentState = State.Done;
                        stateStarted = false;
                    }
                    break;
                case State.Done:
                    SetFeedbackText("Cooling subassembly complete");
                    SetSubassemblyCount(1);
                    currentState = State.GrabFanSupport;
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
        Debug.Log(text);
    }
}
