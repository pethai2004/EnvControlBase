using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using Random = UnityEngine.Random;

public class RobotBasicRun : Robot001Agent
{
    public Transform Destination;
    public Transform bodyRobot;
    public int MaxEnvSteps=10000;
    public float MinAcheiveDistance = 5f;
    public bool internal_observation=false;
    public float TermMinDistance=5f;
    
    [HideInInspector] public float currentRegion;
    [HideInInspector] public BaseEnvParkour baseEnv;
    [HideInInspector] public float CurrentDistance;
    [HideInInspector] public float InitialDistance;
    public int currentSteps=0;
    public ContractDetector HeadCD;
    [HideInInspector] public Vector3 InitialPos;

    void Start(){
        currentRegion = transform.position.z + TermMinDistance;
        baseEnv = GetComponentInParent<BaseEnvParkour>();
        InitialPos = transform.position;
    }
    void FixedUpdate(){
        CurrentDistance = Vector3.Distance(transform.position, Destination.transform.position);
        AddReward(Mathf.Min(CurrentDistance / InitialDistance, 0.1f));
        AddReward(- Mathf.Max(currentSteps / MaxEnvSteps, 0.01f)); // or 1 / MaxEnvSteps costantly
        AddReward(HeadCD.GetCurrent() * -0.05f);
        currentSteps += 1;

        if (CurrentDistance <= MinAcheiveDistance){
            AddReward(1000f);
            ResetAgentScene();
            EndEpisode();
        }
        if (currentSteps % 500 == 0 & currentSteps != 0 & transform.position.z < currentRegion){
            AddReward(-200f);
            ResetAgentScene();
            EndEpisode();
        }
        else if (currentSteps % 500 == 0 & currentSteps != 0 & transform.position.z > currentRegion) {
            currentRegion = transform.position.z + TermMinDistance;
            AddReward(50f); 
        }
        if (transform.position.y < -5f){
            AddReward(-200f);
            ResetAgentScene();
            EndEpisode();
        }
        if (currentSteps >= MaxEnvSteps){
            AddReward(50f);
            ResetAgentScene();
            EpisodeInterrupted();
        }
    }
    public void ResetAgentScene(){
        currentSteps=0;
        transform.position = InitialPos;
        transform.rotation = Quaternion.identity;
        baseEnv.ResetEnvExternal();
    }
    public override void CollectObservations(VectorSensor sensor){
        if (internal_observation){
            obsCollector.AddobservationBodyPart(sensor);
            sensor.AddObservation(CurrentDistance / InitialDistance);
            sensor.AddObservation(transform.position);
            sensor.AddObservation(transform.rotation);
        }
    }
    public override void OnEpisodeBegin(){
        SetupBodyPart();
        transform.position = InitialPos;
    }
}
