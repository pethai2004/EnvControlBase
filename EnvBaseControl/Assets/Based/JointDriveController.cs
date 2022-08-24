using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class AgentController
{
    // a class for each component of Joint that require controller. It responsible for controlling each joint.
    // this use some part of Unity example code
    public ConfigurableJoint joint;
    
    public float current_strength;
    public Vector3 current_rotation;
    public float currentX_rotate;
    public float currentY_rotate;
    public float currentZ_rotate;

    public JointDriveController thisJdController;

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Vector3 startingPos;
    [HideInInspector] public Quaternion startingRot;
    
    public ContractDetector contract_detector;

    public void Reset(AgentController bp)
        {
            bp.rb.transform.position = bp.startingPos;
            bp.rb.transform.rotation = bp.startingRot;
            bp.rb.velocity = Vector3.zero;
            bp.rb.angularVelocity = Vector3.zero;

        }
    public void SetTarget(float x, float y, float z)
    {
        x = (x + 1f) * 0.5f;
        y = (y + 1f) * 0.5f;
        z = (z + 1f) * 0.5f;

        var xRot = Mathf.Lerp(joint.lowAngularXLimit.limit, joint.highAngularXLimit.limit, x);
        var yRot = Mathf.Lerp(-joint.angularYLimit.limit, joint.angularYLimit.limit, y);
        var zRot = Mathf.Lerp(-joint.angularZLimit.limit, joint.angularZLimit.limit, z);

        currentX_rotate = Mathf.InverseLerp(joint.lowAngularXLimit.limit, joint.highAngularXLimit.limit, xRot);
        currentY_rotate = Mathf.InverseLerp(-joint.angularYLimit.limit, joint.angularYLimit.limit, yRot);
        currentZ_rotate = Mathf.InverseLerp(-joint.angularZLimit.limit, joint.angularZLimit.limit, zRot);

        joint.targetRotation = Quaternion.Euler(xRot, yRot, zRot);
        current_rotation = new Vector3(xRot, yRot, zRot);
    }

    public void SetStrength(float strength)
    {
        var rawVal = (strength + 1f) * 0.5f * thisJdController.maxJointForceLimit;
        joint.slerpDrive = new JointDrive {positionSpring = thisJdController.maxJointSpring,
            positionDamper = thisJdController.jointDampen,
            maximumForce = rawVal
        };
        current_strength = rawVal;
    }
}

public class JointDriveController : MonoBehaviour
{

    public float maxJointSpring;
    public float jointDampen;
    public float maxJointForceLimit;
    float m_FacingDot;
    public bool useGroundContact=true;
    
    [HideInInspector] public Dictionary<Transform, AgentController> bodyPartsDict = new Dictionary<Transform, AgentController>();
    [HideInInspector] public List<AgentController> bodyPartsList = new List<AgentController>();

    const float k_MaxAngularVelocity = 50.0f;
    public bool enable_ground_contact=true;

    public void SetupBodyPart(Transform t)
    {
        var bp = new AgentController
        {
            rb = t.GetComponent<Rigidbody>(),
            joint = t.GetComponent<ConfigurableJoint>(),
            //contract_detector = t.gameObject.AddComponent<ContractDetector>(),
            startingPos = t.position,
            startingRot = t.rotation
        };
        bp.rb.maxAngularVelocity = k_MaxAngularVelocity;
        if (bp.joint)
        {
            bp.joint.slerpDrive = new JointDrive
            {
                positionSpring = maxJointSpring, positionDamper = jointDampen, maximumForce = maxJointForceLimit
            };
        }

        bp.thisJdController = this;
        bodyPartsDict.Add(t, bp);
        bodyPartsList.Add(bp);
    }
}

