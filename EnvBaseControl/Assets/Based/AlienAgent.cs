using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using Random = UnityEngine.Random;

public class AlienAgent : Agent
{
    [Range(0.1f, m_maxWalkingSpeed)]
    [SerializeField]
    private float m_TargetWalkingSpeed = m_maxWalkingSpeed;
    const float m_maxWalkingSpeed = 15; //The max walking speeก
    public int action_size=54;
    public float TargetWalkingSpeed
    {
        get { return m_TargetWalkingSpeed; }
        set { m_TargetWalkingSpeed = Mathf.Clamp(value, .1f, m_maxWalkingSpeed); }
    }

    public Transform body;
    public Transform leg0Upper;
    public Transform leg0Lower;
    public Transform leg0but;

    public Transform leg1Upper;
    public Transform leg1Lower;
    public Transform leg1but;

    public Transform leg2Upper;
    public Transform leg2Lower;
    public Transform leg2but;

    public Transform leg3Upper;
    public Transform leg3Lower;
    public Transform leg3but;

    public Transform leg4Upper;
    public Transform leg4Lower;
    public Transform leg4but;

    public Transform leg5Upper;
    public Transform leg5Lower;
    public Transform leg5but;

    JointDriveController m_JdController;
    [HideInInspector] public ObservationCollector obsCollector;
    public List<Transform> Anchors;
    public bool MaximumForce=false;

    public override void Initialize()
    {
        m_JdController = GetComponent<JointDriveController>();
        obsCollector = GetComponent<ObservationCollector>();
        //Setup each body part
        m_JdController.SetupBodyPart(body);
        m_JdController.SetupBodyPart(leg0Upper);
        m_JdController.SetupBodyPart(leg0Lower);
        m_JdController.SetupBodyPart(leg0but);
        m_JdController.SetupBodyPart(leg1Upper);
        m_JdController.SetupBodyPart(leg1Lower);
        m_JdController.SetupBodyPart(leg1but);
        m_JdController.SetupBodyPart(leg2Upper);
        m_JdController.SetupBodyPart(leg2Lower);
        m_JdController.SetupBodyPart(leg2but);
        m_JdController.SetupBodyPart(leg3Upper);
        m_JdController.SetupBodyPart(leg3Lower);
        m_JdController.SetupBodyPart(leg3but);
        m_JdController.SetupBodyPart(leg4Upper);
        m_JdController.SetupBodyPart(leg4Lower);
        m_JdController.SetupBodyPart(leg4but);
        m_JdController.SetupBodyPart(leg5Upper);
        m_JdController.SetupBodyPart(leg5Lower);
        m_JdController.SetupBodyPart(leg5but);
    }

    public void SetupBodyPart(){
        foreach (var bodyPart in m_JdController.bodyPartsDict.Values){
            bodyPart.Reset(bodyPart);
        }
    }
    public override void OnActionReceived(ActionBuffers actionBuffers){
        ControlAgent(actionBuffers);
    }

    public void ControlAgent(ActionBuffers actionBuffers=new ActionBuffers()){ /////54 , 36
        var bpDict = m_JdController.bodyPartsDict;
        var i = -1;
        var continuousActions = actionBuffers.ContinuousActions;
        //var continuousActions = SelfRandomAction();
        
        bpDict[leg0Upper].SetTarget(continuousActions[++i], continuousActions[++i], 0);
        bpDict[leg1Upper].SetTarget(continuousActions[++i], continuousActions[++i], 0);
        bpDict[leg2Upper].SetTarget(continuousActions[++i], continuousActions[++i], 0);
        bpDict[leg3Upper].SetTarget(continuousActions[++i], continuousActions[++i], 0);
        bpDict[leg4Upper].SetTarget(continuousActions[++i], continuousActions[++i], 0);
        bpDict[leg5Upper].SetTarget(continuousActions[++i], continuousActions[++i], 0);

        bpDict[leg0but].SetTarget(continuousActions[++i], 0, 0);
        bpDict[leg1but].SetTarget(continuousActions[++i], 0, 0);
        bpDict[leg2but].SetTarget(continuousActions[++i], 0, 0);
        bpDict[leg3but].SetTarget(continuousActions[++i], 0, 0);
        bpDict[leg4but].SetTarget(continuousActions[++i], 0, 0);
        bpDict[leg5but].SetTarget(continuousActions[++i], 0, 0);

        bpDict[leg0Lower].SetTarget(continuousActions[++i], continuousActions[++i], 0);
        bpDict[leg1Lower].SetTarget(continuousActions[++i], continuousActions[++i], 0);
        bpDict[leg2Lower].SetTarget(continuousActions[++i], continuousActions[++i], 0);
        bpDict[leg3Lower].SetTarget(continuousActions[++i], continuousActions[++i], 0);
        bpDict[leg4Lower].SetTarget(continuousActions[++i], continuousActions[++i], 0);
        bpDict[leg5Lower].SetTarget(continuousActions[++i], continuousActions[++i], 0);

        if (!MaximumForce)
        {
            bpDict[leg0Upper].SetStrength(continuousActions[++i]);
            bpDict[leg1Upper].SetStrength(continuousActions[++i]);
            bpDict[leg2Upper].SetStrength(continuousActions[++i]);
            bpDict[leg3Upper].SetStrength(continuousActions[++i]);
            bpDict[leg4Upper].SetStrength(continuousActions[++i]);
            bpDict[leg5Upper].SetStrength(continuousActions[++i]);

            bpDict[leg0Lower].SetStrength(continuousActions[++i]);
            bpDict[leg1Lower].SetStrength(continuousActions[++i]);
            bpDict[leg2Lower].SetStrength(continuousActions[++i]);
            bpDict[leg3Lower].SetStrength(continuousActions[++i]);
            bpDict[leg4Lower].SetStrength(continuousActions[++i]);
            bpDict[leg5Lower].SetStrength(continuousActions[++i]);

            bpDict[leg0but].SetStrength(continuousActions[++i]);
            bpDict[leg1but].SetStrength(continuousActions[++i]);
            bpDict[leg2but].SetStrength(continuousActions[++i]);
            bpDict[leg3but].SetStrength(continuousActions[++i]);
            bpDict[leg4but].SetStrength(continuousActions[++i]);
            bpDict[leg5but].SetStrength(continuousActions[++i]);
        }
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