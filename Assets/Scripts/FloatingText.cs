using UnityEngine;

public class FloatingText : MonoBehaviour
{
    Transform unit;
    Transform mainCam;
    Transform WorldSpaceCanvas;
    CanvasGroup cGroup;

    public Vector3 offset;

    [SerializeField] SimManagerScript manager;
    
    public int stationNum;
    
    public int prevStationVal;
    
    void Awake()
    {
        stationNum = transform.parent.GetSiblingIndex();
        cGroup = GetComponent<CanvasGroup>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCam = Camera.main.transform;
        unit = transform.parent;
        WorldSpaceCanvas = GameObject.Find("WorldCanvas").transform;

        transform.SetParent(WorldSpaceCanvas);
        prevStationVal = manager.stationVal;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
        transform.position = unit.position + offset;

        if(stationNum == manager.stationVal)
        {
            cGroup.alpha = 1;
        }
        else
        {
            cGroup.alpha = 0;
        }
        prevStationVal = manager.stationVal;
    }
}
