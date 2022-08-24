using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Random = UnityEngine.Random;


public class AgentRun002Multi : AlienAgent
{
    public GameObject TargetObject; 
    public bool DynamicTarget=false;
    [HideInInspector] public Vector3 InitialPos;
    [HideInInspector] public float InitialDistance;
    [HideInInspector] public MultAgentSim multSim;
    [HideInInspector] public int AgentIDk;
    void Start(){
        //multSim = GetComponentInParent<MultAgentSim>();
        InitialDistance = multSim.MinSpawnTargArea; // have to set default value since it will output Nan at first step
    }
    public override void CollectObservations(VectorSensor sensor){
        obsCollector.AddobservationBodyPart(sensor);
        obsCollector.AddAvgJointVeloc(sensor);
        if (Anchors != null){
            obsCollector.AddConnectedAnchor(sensor, Anchors);
        }
        // sensor.AddObservation(transform.position);
        // sensor.AddObservation(transform.rotation);
        sensor.AddObservation(Vector3.Distance(InitialPos, transform.position) / multSim.MaxAreaDistance);
        sensor.AddObservation(Vector3.Distance(transform.position, TargetObject.transform.position) / multSim.MaxAreaDistance);
        sensor.AddObservation(TargetObject.transform.position.normalized);
    }
    public override void OnEpisodeBegin(){
        InitialPos = transform.position;
        SetupBodyPart();
        InitialDistance = Vector3.Distance(InitialPos, TargetObject.transform.position);
    }
    void FixedUpdate(){
        AddReward(- Vector3.Distance(transform.position, TargetObject.transform.position) / InitialDistance);
        if (transform.position.x > multSim.MaxAreaDistance || transform.position.z > multSim.MaxAreaDistance){
            AddReward(-1000f);
            transform.position = new Vector3(0f, 5f, 0f); //agent out of map
        }
        AddReward(Mathf.Max(- multSim.deltaTimeHit/multSim.MaxEnvSteps, - 100 * (1/multSim.MaxEnvSteps)));
    }
}
