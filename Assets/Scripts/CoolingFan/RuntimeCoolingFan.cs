using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using TMPro;

public class RuntimeCoolingFan : MonoBehaviour
{
    public float grabMaterialTime = 2f;
    public float screwTime = 8f;
    public float screwFailRate = 0.2f;
    public bool runtimeStarted = false;
    
    public RuntimeFanSupport runtimeFanSupport;
    public RuntimeFanCrimping runtimeFanCrimping;
    public TextMeshProUGUI subassemblyCountText;
    public TextMeshProUGUI feedbackText;

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
    private int screwCount= 0;
    private int screwedFanCount = 0;
    private int subassemblyCount = 0;
    private float currentTime = 0;

    public GameObject workingObject;
    Animator objectAnim;

    void Start(){
        workingObject.SetActive(false);
    }

    void Update() {
        if (runtimeStarted) {
            int coolingSubassemblyCount = runtimeFanSupport.GetSubassemblyCount();
            int fanSubassemblyCount = runtimeFanCrimping.GetSubassemblyCount();
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
        Debug.Log(text);
    }
}
