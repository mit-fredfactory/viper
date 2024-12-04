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

    void Update() {
        if (runtimeStarted) {
            switch (currentState) {
                case State.GrabFanSupport:
                    if (!stateStarted) {
                        stateStartTime = Time.time;
                        stateStarted = true;
                        m5InsertCount = 0;
                        m3InsertCount = 0;
                        Debug.Log("Grabbing fan support");
                    }
                    if (Time.time - stateStartTime > grabMaterialTime) {
                        currentState = State.PositionFanSupportOnJig;
                        stateStarted = false;
                    }
                    break;
                case State.PositionFanSupportOnJig:
                    if (!stateStarted) {
                        stateStartTime = Time.time;
                        stateStarted = true;
                        Debug.Log("Positioning fan support on jig");
                    }
                    if (Time.time - stateStartTime >
                        positionFanSupportOnJigTime) {
                        currentState = State.M5Insert;
                        stateStarted = false;
                    }
                    break;
                case State.M5Insert:
                    if (!stateStarted) {
                        stateStartTime = Time.time;
                        stateStarted = true;
                        Debug.Log("Grabbing M5 insert");
                    }
                    if (stateStarted && Time.time - stateStartTime > 
                        grabMaterialTime) {
                        if (!heatInsertStarted) {
                            heatInsertStartTime = Time.time;
                            heatInsertStarted = true;
                            Debug.Log("Inserting M5 insert");
                        }
                        if (Time.time - heatInsertStartTime > addHeatInsertTime) {
                            if (Random.value < heatInsertFailureRate) {
                                Debug.Log("M5 insert failed, subassembly to scrap");
                                currentState = State.GrabFanSupport;
                            } else {
                                Debug.Log("M5 insert successful");
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
                        stateStartTime = Time.time;
                        stateStarted = true;
                        Debug.Log("Grabbing M3 insert");
                    }
                    if (stateStarted && Time.time - stateStartTime > 
                        grabMaterialTime) {
                        if (!heatInsertStarted) {
                            heatInsertStartTime = Time.time;
                            heatInsertStarted = true;
                            Debug.Log("Inserting M3 insert");
                        }
                        if (Time.time - heatInsertStartTime > addHeatInsertTime) {
                            if (Random.value < heatInsertFailureRate) {
                                Debug.Log("M3 insert failed, Subassembly to scrap");
                                currentState = State.GrabFanSupport;
                            } else {
                                Debug.Log("M3 insert successful");
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
                    Debug.Log("Cooling subassembly complete");
                    subassemblyCount++;
                    subassemblyCountText.text = subassemblyCount.ToString();
                    currentState = State.GrabFanSupport;
                    break;
            }
        }
    }
}
