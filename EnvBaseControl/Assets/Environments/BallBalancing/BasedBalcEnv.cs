using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.MLAgents;

public class BasedBalcEnv : MonoBehaviour
{

    public Transform Baseplate;
    public Transform AgentTransform;
    public int MaxEnvSteps=2000;
    public float TresholdHeight=10f;
    //public bool ApplyTranslation=false; //not implemented yet!!
    public float force=100f;
    public int addForaceInterval=100;
    public float decayForce=0.95f;
    [HideInInspector] public int envSteps=0;
    [HideInInspector] public Agent agentBL;
    [HideInInspector] public Vector3 StartingPos;
    [HideInInspector] public Vector3 currentForce=Vector3.zero;
    
    void Start(){
        agentBL = AgentTransform.GetComponent<Agent>();
        StartingPos = AgentTransform.position;
    }
    void ResetEnv(){
        envSteps = 0;
        AgentTransform.position = StartingPos;
        AgentTransform.rotation = Quaternion.Euler(0, Random.Range(0, 306), 0);
        Baseplate.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        Baseplate.rotation = Quaternion.identity;
    }
    void RotateBaseplate(){
        var plateRB = Baseplate.GetComponent<Rigidbody>();
        var multiplier = Random.Range(0.6f, 1f);
        plateRB.AddTorque(currentForce * multiplier, ForceMode.Force);
    }
    void FixedUpdate(){
        if (envSteps % addForaceInterval == 0){
            currentForce = new Vector3(Random.Range(-force, force), Random.Range(-force/2, force/2), Random.Range(-force, force));
        }
        envSteps += 1;
        if (envSteps >= MaxEnvSteps){
            agentBL.AddReward(1f);
            agentBL.EpisodeInterrupted();
            ResetEnv();
        }
        if (AgentTransform.position.y <= TresholdHeight){
            agentBL.AddReward(-1f);
            agentBL.EndEpisode();
            ResetEnv();
        }
    }
    void Update(){
    currentForce.x *= decayForce;
    currentForce.y *= decayForce;
    currentForce.z *= decayForce;
    RotateBaseplate();
    }
}
