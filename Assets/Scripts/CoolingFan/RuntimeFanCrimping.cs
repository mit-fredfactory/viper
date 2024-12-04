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

    void Update() {
        if (runtimeStarted) {
            switch (currentState) {
                case State.GrabFan:
                    if (!stateStarted) {
                        stateStartTime = Time.time;
                        stateStarted = true;
                        Debug.Log("Grabbing fan");
                    }
                    if (Time.time - stateStartTime > grabMaterialTime) {
                        currentState = State.GrabWire;
                        stateStarted = false;
                    }
                    break;
                case State.GrabWire:
                    if (!stateStarted) {
                        stateStartTime = Time.time;
                        stateStarted = true;
                        Debug.Log("Grabbing wire");
                    }
                    if (Time.time - stateStartTime > grabMaterialTime) {
                        currentState = State.CrimpWire;
                        stateStarted = false;
                    }
                    break;
                case State.CrimpWire:
                    if (!stateStarted) {
                        stateStartTime = Time.time;
                        stateStarted = true;
                        Debug.Log("Crimping wire");
                    }
                    if (Time.time - stateStartTime > crimpWireTime) {
                        if (Random.value < crimpWireFailRate) {
                            Debug.Log("Wire crimping failed, retrying with new fan");
                            currentState = State.GrabFan;
                        } else {
                            Debug.Log("Wire crimping successful");
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
                        stateStartTime = Time.time;
                        stateStarted = true;
                        Debug.Log("Moving subassembly");
                    }
                    if (Time.time - stateStartTime > moveSubassemblyTime) {
                        currentState = State.Done;
                        stateStarted = false;
                    }
                    break;
                case State.Done:
                    Debug.Log("Fan subassembly complete");
                    subassemblyCount++;
                    subassemblyCountText.text = subassemblyCount.ToString();
                    currentState = State.GrabFan;
                    break;
            }
        }
    }
}
