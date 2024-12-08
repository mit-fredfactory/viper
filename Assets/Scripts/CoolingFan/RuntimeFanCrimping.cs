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
    private float currentTime = 0;

    void Start(){
        workingObject[0].SetActive(false);
        workingObject[1].SetActive(false);
    }

    void Update() {
        if (runtimeStarted) {
            switch (currentState) {
                case State.GrabFan:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                        SetFeedbackText("Grabbing fan");
                        workingObject[crimpedFanCount].SetActive(false);
                        workingObject[crimpedFanCount].SetActive(true);
                        objectAnim[crimpedFanCount].SetFloat("GFMultiplier", manager.speed/grabMaterialTime);
                    }
                    if (currentTime - stateStartTime > grabMaterialTime) {
                        currentState = State.GrabWire;
                        stateStarted = false;
                    }
                    else
                    {
                        objectAnim[crimpedFanCount].SetFloat("GFMultiplier", manager.speed/grabMaterialTime);
                    }
                    break;
                case State.GrabWire:
                    if (!stateStarted) {
                        stateStartTime = currentTime;
                        stateStarted = true;
                        SetFeedbackText("Grabbing wire");
                        objectAnim[crimpedFanCount].SetFloat("GWMultiplier", manager.speed/grabMaterialTime);
                    }
                    if (currentTime - stateStartTime > grabMaterialTime) {
                        currentState = State.CrimpWire;
                        stateStarted = false;
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
                    workingObject[0].SetActive(false);
                    workingObject[1].SetActive(false);
                    break;
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
        Debug.Log(text);
    }
}
