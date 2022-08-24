using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Random = UnityEngine.Random;

public class MultAgentSim : MonoBehaviour
{
    public int NumberAgents=100;
    public GameObject AgentPrefabs; // alienAgent only
    public GameObject TargetObjectML;
    public float AgentSpawnArea=10f;
    public float MinSpawnTargArea=40; //Min Distance to agent (each)
    public int MaxEnvSteps=10000;
    public float AddPunishment=0.5f; //constant multiplier reward if other agent hit target

    [HideInInspector] public int GlobalEnvSteps=0;
    [HideInInspector] public float MaxSpawnTargArea=70f;
    [HideInInspector] public float MaxAreaDistance=100f;
    [HideInInspector] public List<AgentRun002Multi> AgentSims = new List<AgentRun002Multi>();
    [HideInInspector] public List<int> acquiredCount = new List<int>();
    [HideInInspector] public int deltaTimeHit=0;
    int GetAllPastHit(){
        var sum=0;
        foreach (var values in acquiredCount){
            sum += values;
        }
        return sum;
    }
    public void resetTargetPos(){ //reset target position after been hit/reset episode
        Vector3 pos = new Vector3(0,0,0);
        foreach (AgentRun002Multi ag in AgentSims){
            pos += ag.transform.position;
        }
        pos = pos / AgentSims.Count;
        float xp = pos.x + MinSpawnTargArea;
        float zp = pos.z + MinSpawnTargArea;
        if (xp  > MaxSpawnTargArea){
            xp = xp - 2 *MinSpawnTargArea;
        }
        if (xp < - MinSpawnTargArea){
            xp = xp + 2 * MinSpawnTargArea;
        }
        if (zp  > MaxSpawnTargArea){
            zp = zp - 2 * MinSpawnTargArea;
        }
        if (zp < - MinSpawnTargArea){
            zp = zp + 2 * MinSpawnTargArea;
        }
        var newP = new Vector3(xp, 5, zp);
        TargetObjectML.transform.position = newP;
    }
    public void resetAgentPos(){ //reset all agents position
        foreach (AgentRun002Multi ag in AgentSims){
            var pos = new Vector3(Random.Range(-AgentSpawnArea, AgentSpawnArea), 5, 
                Random.Range(-AgentSpawnArea, AgentSpawnArea));
            ag.transform.position = pos;
            ag.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        }
    }
    void FixedUpdate(){
        GlobalEnvSteps+=1;
        deltaTimeHit+=1;
        var CompoTarg = TargetObjectML.GetComponent<TargetDs>();
        if (CompoTarg.Contracted & CompoTarg.ag != null){
            var agk = CompoTarg.ag.GetComponentInParent<AgentRun002Multi>();
            agk.AddReward(3f); // reward for agent hit target
            acquiredCount[agk.AgentIDk] += 1;
            foreach (var ag in AgentSims){  // reward for agent miss target
                if (ag != agk){ 
                    ag.AddReward(-3f * AddPunishment);
                }
            }
            resetTargetPos();
        }
        if (GlobalEnvSteps >= MaxEnvSteps){
            var allPasthits = GetAllPastHit();
            GlobalEnvSteps = 0;
            resetAgentPos();
            resetTargetPos();
            foreach (var ag in AgentSims){
                ag.AddReward(-1f);
                acquiredCount[ag.AgentIDk] = 0;
                ag.EpisodeInterrupted();
            }
        }
    }
    public void InitializeAgents(){
        for (int i=0; i<NumberAgents; i++){
            var Pos = new Vector3(Random.Range(-AgentSpawnArea, AgentSpawnArea), 7, 
                Random.Range(-AgentSpawnArea, AgentSpawnArea));
            var Rot = Quaternion.Euler(0, Random.Range(0, 360), 0);
            GameObject new0 = (GameObject) Instantiate(AgentPrefabs, Pos, Rot);
            var agent_com = new0.GetComponent<AgentRun002Multi>();
            TargetObjectML.transform.position = new Vector3(Random.Range(-MinSpawnTargArea, MinSpawnTargArea), 5, 
                                                    Random.Range(-MinSpawnTargArea, MinSpawnTargArea));
            agent_com.TargetObject = TargetObjectML;
            agent_com.multSim = this;
            agent_com.InitialPos = Pos;
            agent_com.AgentIDk = i;
            
            AgentSims.Add(agent_com);
            acquiredCount.Add(0);
        }
    }
    public void Start(){
        InitializeAgents();
    }
}