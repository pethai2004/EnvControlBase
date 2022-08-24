using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using Random = UnityEngine.Random;

public class Robotballbalance : Robot001Agent
{
    [HideInInspector] public BasedBalcEnv baseBLenv;
    [HideInInspector] public ContractDetector contractDetector;
    //[HideInInspector] ObservationCollector obsCollector;

    void Start(){
        baseBLenv = GetComponentInParent<BasedBalcEnv>();
        contractDetector = GetComponent<ContractDetector>();
        //obsCollector = GetComponent<ObservationCollector>();
    }
    // public override void CollectObservations(VectorSensor sensor){
    //     sensor.AddObservation(baseBLenv.Baseplate.transform.rotation.normalized);
    //     sensor.AddObservation(baseBLenv.Baseplate.GetComponent<Rigidbody>().angularVelocity.normalized);
    //     obsCollector.AddobservationBodyPart(sensor);
    // }
    void FixedUpdate(){
        AddReward(contractDetector.GetCurrent() * - 0.001f);
        AddReward( 1 / baseBLenv.MaxEnvSteps);
    }
}
