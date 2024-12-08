using UnityEngine;

public class SimManagerScript : MonoBehaviour
{
    [HideInInspector] public int stationVal;
    public GameObject stations;
    public GameObject cameraObject;
    public GameObject[] startUI;

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
        cameraObject.transform.position = Vector3.Lerp(cameraObject.transform.position, stations.transform.GetChild(stationVal).GetChild(0).position, 2f * Time.deltaTime);
        cameraObject.transform.rotation = Quaternion.Lerp(cameraObject.transform.rotation, stations.transform.GetChild(stationVal).GetChild(0).rotation, 1.75f * Time.deltaTime);
    }

    public void IncreaseStationVal()
    {
        stationVal++;
        if(stationVal > stations.transform.childCount - 1)
        {
            stationVal = 0;
        }
        StartUIEnable();
    }

    public void DecreaseStationVal()
    {
        stationVal--;
        if(stationVal < 0)
        {
            stationVal = stations.transform.childCount - 1;
        }
        StartUIEnable();
    }

    public void StartUIEnable()
    {
        if(stationVal == 0)
        {
            for(int i = 0; i < startUI.Length; i++)
            {
                startUI[i].SetActive(true);
            }
        }
        else
        {
            for(int i = 0; i < startUI.Length; i++)
            {
                startUI[i].SetActive(false);
            }
        }
    }
}
