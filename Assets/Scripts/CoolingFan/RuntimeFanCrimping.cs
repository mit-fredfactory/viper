using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using TMPro;

public class RuntimeFanCrimping : MonoBehaviour
{
    public float grabMaterialTime = 2f;
    public float crimpWireTime = 8f;
    public float crimpWireFailRate = 0.4f;
    public float moveSubassemblyTime = 5f;

    public bool runtimeStarted = false;
    
    public TextMeshProUGUI subassemblyCountText;
    public TextMeshProUGUI feedbackText;

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
    private float currentTime = 0;

    void Update() {
        if (runtimeStarted) {
            switch (currentState) {
                case State.GrabFan:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                        SetFeedbackText("Grabbing fan");
                    }
                    if (currentTime - stateStartTime > grabMaterialTime) {
                        currentState = State.GrabWire;
                        stateStarted = false;
                    }
                    break;
                case State.GrabWire:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                        SetFeedbackText("Grabbing wire");
                    }
                    if (currentTime - stateStartTime > grabMaterialTime) {
                        currentState = State.CrimpWire;
                        stateStarted = false;
                    }
                    break;
                case State.CrimpWire:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                        SetFeedbackText("Crimping wire");
                    }
                    if (currentTime - stateStartTime > crimpWireTime) {
                        if (Random.value < crimpWireFailRate) {
                            SetFeedbackText("Wire crimping failed, retrying with new fan");
                            currentState = State.GrabFan;
                        } else {
                            SetFeedbackText("Wire crimping successful");
                            crimpedFanCount++;
                            if (crimpedFanCount == 2) {
                                currentState = State.MoveSubassembly;
                                crimpedFanCount = 0;
                            }    
                        }
                        stateStarted = false;
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
                    SetFeedbackText("Fan subassembly complete");
                    subassemblyCount++;
                    subassemblyCountText.text = subassemblyCount.ToString();
                    currentState = State.GrabFan;
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
}
