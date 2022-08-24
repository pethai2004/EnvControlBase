using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using Random = UnityEngine.Random;

public class HumanoidAgent : Agent
{
    [Range(0.1f, 10)]
    public float m_TargetWalkingSpeed = 10;

    public float MTargetWalkingSpeed 
    {
        get { return m_TargetWalkingSpeed; }
        set { m_TargetWalkingSpeed = Mathf.Clamp(value, .1f, m_maxWalkingSpeed); }
    }

    const float m_maxWalkingSpeed = 10; 

    //The direction an agent will walk during training.
    private Vector3 m_WorldDirToWalk = Vector3.right;
    [HideInInspector] public int contractCounter=0;
    public int action_size=39;

    public Transform hips;
    public Transform chest;
    public Transform spine;
    public Transform head;
    public Transform thighL;
    public Transform shinL;
    public Transform footL;
    public Transform thighR;
    public Transform shinR;
    public Transform footR;
    public Transform armL;
    public Transform forearmL;
    public Transform handL;
    public Transform armR;
    public Transform forearmR;
    public Transform handR;

    JointDriveController m_JdController;
    EnvironmentParameters m_ResetParams;
    [HideInInspector] public ObservationCollector obsCollector;
    public List<Transform> Anchors;
    public bool MaximumForce=false;

    public override void Initialize()
    {
        m_JdController = GetComponent<JointDriveController>();
        obsCollector = GetComponent<ObservationCollector>();

        m_JdController.SetupBodyPart(hips);
        m_JdController.SetupBodyPart(chest);
        m_JdController.SetupBodyPart(spine);
        m_JdController.SetupBodyPart(head);
        m_JdController.SetupBodyPart(thighL);
        m_JdController.SetupBodyPart(shinL);
        m_JdController.SetupBodyPart(footL);
        m_JdController.SetupBodyPart(thighR);
        m_JdController.SetupBodyPart(shinR);
        m_JdController.SetupBodyPart(footR);
        m_JdController.SetupBodyPart(armL);
        m_JdController.SetupBodyPart(forearmL);
        m_JdController.SetupBodyPart(handL);
        m_JdController.SetupBodyPart(armR);
        m_JdController.SetupBodyPart(forearmR);
        m_JdController.SetupBodyPart(handR);

        m_ResetParams = Academy.Instance.EnvironmentParameters;
        SetResetParameters();
    }
    public void SetupBodyPart(){
        foreach (var bodyPart in m_JdController.bodyPartsDict.Values){
            bodyPart.Reset(bodyPart);
        }
    }
    public void ControlAgent(ActionBuffers actionBuffers=new ActionBuffers())
    {
        var bpDict = m_JdController.bodyPartsDict;
        var i = -1;
        var continuousActions = actionBuffers.ContinuousActions;
        //var continuousActions = SelfRandomAction();

        bpDict[chest].SetTarget(continuousActions[++i], continuousActions[++i], continuousActions[++i]);
        bpDict[spine].SetTarget(continuousActions[++i], continuousActions[++i], continuousActions[++i]);

        bpDict[thighL].SetTarget(continuousActions[++i], continuousActions[++i], 0);
        bpDict[thighR].SetTarget(continuousActions[++i], continuousActions[++i], 0);
        bpDict[shinL].SetTarget(continuousActions[++i], 0, 0);
        bpDict[shinR].SetTarget(continuousActions[++i], 0, 0);
        bpDict[footR].SetTarget(continuousActions[++i], continuousActions[++i], continuousActions[++i]);
        bpDict[footL].SetTarget(continuousActions[++i], continuousActions[++i], continuousActions[++i]);

        bpDict[armL].SetTarget(continuousActions[++i], continuousActions[++i], 0);
        bpDict[armR].SetTarget(continuousActions[++i], continuousActions[++i], 0);
        bpDict[forearmL].SetTarget(continuousActions[++i], 0, 0);
        bpDict[forearmR].SetTarget(continuousActions[++i], 0, 0);
        bpDict[head].SetTarget(continuousActions[++i], continuousActions[++i], 0);

        //update joint strength settings
        if (!MaximumForce){
            bpDict[chest].SetStrength(continuousActions[++i]);
            bpDict[spine].SetStrength(continuousActions[++i]);
            bpDict[head].SetStrength(continuousActions[++i]);
            bpDict[thighL].SetStrength(continuousActions[++i]);
            bpDict[shinL].SetStrength(continuousActions[++i]);
            bpDict[footL].SetStrength(continuousActions[++i]);
            bpDict[thighR].SetStrength(continuousActions[++i]);
            bpDict[shinR].SetStrength(continuousActions[++i]);
            bpDict[footR].SetStrength(continuousActions[++i]);
            bpDict[armL].SetStrength(continuousActions[++i]);
            bpDict[forearmL].SetStrength(continuousActions[++i]);
            bpDict[armR].SetStrength(continuousActions[++i]);
            bpDict[forearmR].SetStrength(continuousActions[++i]);
        }
    }
    public override void OnActionReceived(ActionBuffers actionBuffers){
        ControlAgent(actionBuffers);
    }
    public void SetTorsoMass(){
        m_JdController.bodyPartsDict[chest].rb.mass = m_ResetParams.GetWithDefault("chest_mass", 8);
        m_JdController.bodyPartsDict[spine].rb.mass = m_ResetParams.GetWithDefault("spine_mass", 8);
        m_JdController.bodyPartsDict[hips].rb.mass = m_ResetParams.GetWithDefault("hip_mass", 8);
    }
    public void SetResetParameters(){
        SetTorsoMass();
    }
    public List<float> SelfRandomAction(){
        List<float> test2 = new List<float>();
        for (int i = 0; i < action_size; i++)
        {
            test2.Add(Random.Range(0f, 1f));
        }
        return test2;
    }
}
