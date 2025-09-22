using UnityEngine;
using UnityEngine.EventSystems;

public class UIHideShow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float targetXPos;
    [SerializeField] private float targetYPos;
    [SerializeField] private float moveSpeed;
    private Vector3 initialPos;
    private Vector3 targetPos;
    private float progress = 0f;
    private bool touching = false;
    private RectTransform RT;

    void Start()
    {
        RT = GetComponent<RectTransform>();
        initialPos = RT.localPosition;
        targetPos = new Vector3(targetXPos, targetYPos, RT.localPosition.z);
    }

    void Update()
    {
        if(touching == true && progress < 1){
            progress = progress + Time.deltaTime * moveSpeed;
            RT.localPosition = Vector3.Lerp(initialPos, targetPos, progress);
        }
        else if(touching == false && progress > 0){
            progress = progress - Time.deltaTime * moveSpeed;
            RT.localPosition = Vector3.Lerp(targetPos, initialPos, 1-progress);
        }
    }

    public void OnPointerEnter(PointerEventData eventData){
        touching = true;
    }

    public void OnPointerExit(PointerEventData eventData){
        touching = false;
    }
}
