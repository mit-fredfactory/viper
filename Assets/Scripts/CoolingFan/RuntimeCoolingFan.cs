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
    
    public TextMeshProUGUI coolingSubassemblyCountText;
    public TextMeshProUGUI fanSubassemblyCountText;
    public TextMeshProUGUI subassemblyCountText;

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

    void Update() {
        if (runtimeStarted) {
            int coolingSubassemblyCount = int.Parse(
                                            coolingSubassemblyCountText.text);
            int fanSubassemblyCount = int.Parse(
                                            fanSubassemblyCountText.text);
            switch (currentState) {
                case State.GrabCoolingSubassembly:
                    if (!stateStarted) {
                        if (coolingSubassemblyCount > 0) {
                            stateStartTime = Time.time;
                            stateStarted = true;
                            Debug.Log("Grabbing cooling subassembly");
                            coolingSubassemblyCountText.text = 
                                (coolingSubassemblyCount - 1).ToString();
                        } else {
                            Debug.Log("No cooling subassemblies available");
                        }
                    } else if (Time.time - stateStartTime > grabMaterialTime) {
                        currentState = State.GrabFanSubassembly;
                        stateStarted = false;
                    }
                    break;
                case State.GrabFanSubassembly:
                    if (!stateStarted) {
                        if (fanSubassemblyCount > 0) {
                            stateStartTime = Time.time;
                            stateStarted = true;
                            Debug.Log("Grabbing fan subassembly");
                            fanSubassemblyCountText.text = 
                                (fanSubassemblyCount - 1).ToString();
                        } else {
                            Debug.Log("No fan subassemblies available");
                        }
                    } else if (Time.time - stateStartTime > grabMaterialTime) {
                        currentState = State.Screw;
                        stateStarted = false;
                    }
                    break;
                case State.Screw:
                    if (!stateStarted) {
                        stateStartTime = Time.time;
                        stateStarted = true;
                        Debug.Log("Screwing");
                    }
                    if (Time.time - stateStartTime > screwTime) {
                        if (Random.value < screwFailRate) {
                            Debug.Log("Screwing failed, retrying");
                        } else {
                            Debug.Log("Screw successful");
                            screwCount++;
                            if (screwCount == 2) {
                                Debug.Log("Fan screwed successfully");
                                screwedFanCount++;
                                if (screwedFanCount == 2) {
                                    Debug.Log("Both fans screwed successfully");
                                    currentState = State.Done;
                                } else {
                                    screwCount = 0;
                                }
                                screwCount = 0;
                            }    
                        }
                        stateStarted = false;
                    }
                    break;
                case State.Done:
                    Debug.Log("Cooling Fan subassembly complete");
                    subassemblyCount++;
                    subassemblyCountText.text = subassemblyCount.ToString();
                    currentState = State.GrabCoolingSubassembly;
                    break;
            }
        }
    }
}
