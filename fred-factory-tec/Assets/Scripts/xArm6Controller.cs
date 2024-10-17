using UnityEngine;

public class xArm6Controller : MonoBehaviour
{
    public Transform baseJoint;
    public Transform shoulderJoint;
    public Transform elbowJoint;
    public Transform wrist1Joint;
    public Transform wrist2Joint;
    public Transform wrist3Joint;
    public Transform endEffectorJoint;

    public float baseSpeed = 30f;
    public float shoulderSpeed = 20f;
    public float elbowSpeed = 30f;
    public float wrist1Speed = 40f;
    public float wrist2Speed = 40f;
    public float wrist3Speed = 50f;
    public float endEffectorSpeed = 60f;

    private void Update()
    {
        // Base joint rotation
        if (Input.GetKey(KeyCode.Q))
            baseJoint.Rotate(Vector3.up, baseSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.E))
            baseJoint.Rotate(Vector3.up, -baseSpeed * Time.deltaTime);

        // Shoulder joint rotation
        if (Input.GetKey(KeyCode.W))
            shoulderJoint.Rotate(Vector3.up, shoulderSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.S))
            shoulderJoint.Rotate(Vector3.up, -shoulderSpeed * Time.deltaTime);

        // Elbow joint rotation
        if (Input.GetKey(KeyCode.A))
            elbowJoint.Rotate(Vector3.right, elbowSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.D))
            elbowJoint.Rotate(Vector3.right, -elbowSpeed * Time.deltaTime);

        // Wrist 1 joint rotation
        if (Input.GetKey(KeyCode.R))
            wrist1Joint.Rotate(Vector3.right, wrist1Speed * Time.deltaTime);
        if (Input.GetKey(KeyCode.F))
            wrist1Joint.Rotate(Vector3.right, -wrist1Speed * Time.deltaTime);

        // Wrist 2 joint rotation
        if (Input.GetKey(KeyCode.T))
            wrist2Joint.Rotate(Vector3.up, wrist2Speed * Time.deltaTime);
        if (Input.GetKey(KeyCode.G))
            wrist2Joint.Rotate(Vector3.up, -wrist2Speed * Time.deltaTime);

        // Wrist 3 joint rotation
        if (Input.GetKey(KeyCode.Y))
            wrist3Joint.Rotate(Vector3.right, wrist3Speed * Time.deltaTime);
        if (Input.GetKey(KeyCode.H))
            wrist3Joint.Rotate(Vector3.right, -wrist3Speed * Time.deltaTime);

        // End effector rotation
        if (Input.GetKey(KeyCode.U))
            endEffectorJoint.Rotate(Vector3.up, endEffectorSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.J))
            endEffectorJoint.Rotate(Vector3.up, -endEffectorSpeed * Time.deltaTime);
    }
}
