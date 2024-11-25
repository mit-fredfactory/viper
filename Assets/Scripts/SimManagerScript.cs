using UnityEngine;

public class SimManagerScript : MonoBehaviour
{
    [HideInInspector] public int stationVal;
    public GameObject stations;
    public GameObject cameraObject;

    void Awake()
    {
        stationVal = 0;
    }

    void Start()
    {
        cameraObject.transform.position = stations.transform.GetChild(stationVal).GetChild(0).position;
    }

    void Update()
    {
        cameraObject.transform.position = stations.transform.GetChild(stationVal).GetChild(0).position;
        Debug.Log(stationVal);
    }

    public void IncreaseStationVal()
    {
        stationVal++;
        if(stationVal > stations.transform.childCount - 1)
        {
            stationVal = 0;
        }
    }

    public void DecreaseStationVal()
    {
        stationVal--;
        if(stationVal < 0)
        {
            stationVal = stations.transform.childCount - 1;
        }
    }
}
