using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class just1jointmovemente : MonoBehaviour
{

    public float baseSpeed = 30f;

    // Update is called once per frame
    void Update()
    {
        rotateJoint();
    }

    void rotateJoint()
    {
        float xValue = Input.GetAxis("Horizontal") * Time.deltaTime * baseSpeed;
        transform.Rotate(0,xValue,0);
    }
}
