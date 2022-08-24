using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeControl : MonoBehaviour
{
    public Transform Arm;
    public WheelCollider FrontWheel;
    public WheelCollider BackWheel;
    public Transform FrontWheelT;
    public Transform BackWheelT;
    public Transform Pedal;

    public bool enable_manual=true; 
    public float m_steer=0f;
    public float max_steer=0f;
    public float force=30f;
    public float gear=2f;
    public float c_steering=300f;
    void FixedUpdate()
    {
        if (enable_manual){
            var m_steer = Input.GetAxis("Horizontal");
            var m_vert = Input.GetAxis("Vertical");
            UpdateWheel(m_steer, m_vert);
        }
    }

    public void UpdateWheel(float input_steer, float input_force)
    {
        //FrontWheel.steerAngle = max_steer * input_steer;
        
        BackWheel.motorTorque = input_force * force;
        VisualWheel(FrontWheel, FrontWheelT);
        VisualWheel(BackWheel, BackWheelT);
        VisualWheel(BackWheel, Pedal);
    }
    public void VisualWheel(WheelCollider c, Transform t)
    {
        Vector3 _pos = t.position;
        Quaternion _quat = t.rotation;
        c.GetWorldPose(out _pos, out _quat);
        //t.position = _pos;
        t.rotation = _quat;
    }
}
