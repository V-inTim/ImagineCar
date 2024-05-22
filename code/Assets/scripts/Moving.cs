using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public WheelCollider[] wheelColliders = new WheelCollider[4];
    public Transform[] wheelMeshes = new Transform[4];
    public float maxTorque = 200f;
    public float maxSteerAngle = 30f;

    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    void Update()
    {
        UpdateMeshesPositions();
    }

    void FixedUpdate()
    {
        Drive();
        Steer();
    }

    void Drive()
    {
        float torque = maxTorque * Input.GetAxis("Vertical");
        wheelColliders[2].motorTorque = torque;
        wheelColliders[3].motorTorque = torque;
    }


    void Steer()
    {
        float steerAngle = maxSteerAngle * Input.GetAxis("Horizontal");
        float smoothSteerAngle = Mathf.SmoothDampAngle(wheelColliders[0].steerAngle, steerAngle, ref turnSmoothVelocity, turnSmoothTime);
        wheelColliders[0].steerAngle = smoothSteerAngle;
        wheelColliders[1].steerAngle = smoothSteerAngle;
    }

    void UpdateMeshesPositions()
    {
        for (int i = 0; i < 4; i++)
        {
            Quaternion quat;
            Vector3 pos;
            wheelColliders[i].GetWorldPose(out pos, out quat);
            wheelMeshes[i].position = pos;
            wheelMeshes[i].rotation = quat;
        }
    }
}
