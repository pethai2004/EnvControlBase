using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    private float m_steer;
    private float m_horizon;
    private float m_vert;
    public bool enable_manual=true;
    public float force=50f;
    public float max_steer=30f;

    public WheelCollider FrontR, FrontL, RearL, RearR;
    //public Transform TFrontR, TFrontL, TRearL, TRearR;
    
    void FixedUpdate()
    {
        if (enable_manual){
            m_steer = Input.GetAxis("Horizontal");
            m_vert = Input.GetAxis("Vertical");
            UpdateWheel(m_steer, m_vert);
        }
    }
    public void UpdateWheel(float input_steer, float input_force)
    {
        var st_angle = max_steer * input_steer;
        FrontR.steerAngle = st_angle;
        FrontL.steerAngle = st_angle;
        
        FrontR.motorTorque = input_force * force;
        FrontL.motorTorque = input_force * force;

        RearL.motorTorque = input_force * force /2f;
        RearR.motorTorque = input_force * force /2f;
        //VisualWheelAll();
    }

    // public void VisualWheel(WheelCollider c, Transform t)
    // // Still working on it!
    // {
    //     Vector3 _pos = t.position;
    //     Quaternion _quat = t.rotation;
    //     c.GetWorldPose(out _pos, out _quat);
    //     _quat = Quaternion.Euler(_quat.x, _quat.y, _quat.z);
    //     t.position = _pos;
    //     t.rotation = _quat;
    // }
    // public void VisualWheelAll()
    // {
    //     VisualWheel(FrontR, TFrontR);
    //     VisualWheel(FrontL, TFrontL);
    //     VisualWheel(RearL, TRearL);
    //     VisualWheel(RearR, TRearR);
    // }
}