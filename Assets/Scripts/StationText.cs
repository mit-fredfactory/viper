using UnityEngine;
using TMPro;

public class StationText : MonoBehaviour
{
    TMP_Text tmp;
    [SerializeField] SimManagerScript sim;
    [SerializeField] GameObject stations;

    void Start()
    {
        tmp = GetComponent<TMP_Text>();
    }

    void Update()
    {
        tmp.text = stations.transform.GetChild(sim.stationVal).name;
        if(sim.stationVal == 0)
        {
            tmp.text = "Switch Stations by pressing the buttons";
        }
    }
}
