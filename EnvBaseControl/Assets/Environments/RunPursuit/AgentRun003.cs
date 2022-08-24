using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Random = UnityEngine.Random;

public class AgentRun003 : AntAgent
{
    public float MaxAreaDistance=25;
    public int MaxEnvSteps=4000; // is not agent MaxStep
    public GameObject TargetObject; 
    public bool DynamicTarget=false;
    public float MinInitialTargDist=15f;
    [HideInInspector] public Vector3 InitialPos;
    [HideInInspector] public int CurrentSteps=0;
    [HideInInspector] public float InitialDistance;

    void Awake(){
        InitialDistance=MinInitialTargDist; // have to set default value since it will output Nan at first step
    }
    public override void CollectObservations(VectorSensor sensor){
        obsCollector.AddobservationBodyPart(sensor);
        obsCollector.AddAvgJointVeloc(sensor);
        if (Anchors != null){
            obsCollector.AddConnectedAnchor(sensor, Anchors);
        }
        sensor.AddObservation(transform.position);
        sensor.AddObservation(transform.rotation);
        sensor.AddObservation(Vector3.Distance(InitialPos, transform.position) / MaxAreaDistance);
        sensor.AddObservation(Vector3.Distance(transform.position, TargetObject.transform.position) / MaxAreaDistance);
        sensor.AddObservation(TargetObject.transform.position.normalized);
    }
    public override void OnEpisodeBegin(){
        transform.rotation = Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);
        transform.position = new Vector3(
            Random.Range(-MaxAreaDistance/3f, MaxAreaDistance/3f), 4f, Random.Range(-MaxAreaDistance/3f, MaxAreaDistance/3f));
        InitialPos = transform.position;
        SetupBodyPart();
        var RandPosX = Random.Range(-MaxAreaDistance * 2f, MaxAreaDistance* 2f);
        for (int Ix=0; Ix < 10; Ix++){
            if (RandPosX < MinInitialTargDist){
                RandPosX = Random.Range(-MaxAreaDistance * 2f, MaxAreaDistance* 2f);
            }
        }
        TargetObject.transform.position =  new Vector3(
            Random.Range(-MaxAreaDistance * 2f, MaxAreaDistance* 2f), 4f, Random.Range(MaxAreaDistance* 2f, MaxAreaDistance* 2f));
        InitialDistance = Vector3.Distance(InitialPos, TargetObject.transform.position);
        CurrentSteps = 0;
    }
    void FixedUpdate(){
        CurrentSteps += 1;
        if (CurrentSteps >= MaxEnvSteps){
            AddReward(-3f);
            EpisodeInterrupted();
        }
        if (TargetObject.GetComponent<TargetDs>().Contracted){
            HitTarget();
        }
        AddReward(- Vector3.Distance(transform.position, TargetObject.transform.position) / InitialDistance);
        AddReward(- (CurrentSteps / MaxEnvSteps) * 0.1f);
    }
    // void OnCollisionEnter(Collision other) {
    //     if (other.transform.CompareTag("TargetObject")){
    //         HitTarget();
    //     }
    // }
    public void HitTarget(){
        AddReward(5f * CurrentSteps/MaxEnvSteps);
        EndEpisode();
    }
}
