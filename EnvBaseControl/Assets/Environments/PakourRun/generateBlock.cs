using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.MLAgents;

public class generateBlock : BaseEnvParkour
{
    public Transform blockPrefabs;
    public Agent agent;
    public int NumBlocks = 50;
    public bool RandomBlock=true; // whether to use all same randomized for entire game experience
    public float MaxLengthScale=30f;
    public float MinLengthScale=8f;
    public float MaxHeight=2f;
    public float MinHeight=-2f;
    public float MaxSpace=5f;
    public float MinSpace=3f;
    public float MaxSlope=20f;
    public float applyslopefreq=0.4f;

    public List<float> z_immediateblock = new List<float>();
    public Camera z_camera;
    public Camera y_camera;

    [HideInInspector] public float prevBlockZ=0; //coordinate z
    [HideInInspector] public float prevBlockY=0; //coordinate y
    [HideInInspector] public List<Transform> listBlock= new List<Transform>();
    [HideInInspector] public List<float> listSpace = new List<float>();

    void Start(){
        GenerateBlock();
        destination.position = listBlock[listBlock.Count - 1].position;
    
    }
    void Update(){
        z_camera.transform.position = new Vector3(z_camera.transform.position.x, z_camera.transform.position.y, agent.transform.position.z);
        y_camera.transform.position = new Vector3(y_camera.transform.position.x, y_camera.transform.position.y, agent.transform.position.z);
    }
    public override void ResetEnvExternal(){
        if (RandomBlock){
            DestroyPastBlocks();
            GenerateBlock();
            destination.position = listBlock[listBlock.Count - 1].position;
        }
    }
    public void DestroyPastBlocks(){
        for (int i = 0; i < listBlock.Count; i++){
            Destroy(listBlock[i].gameObject);
        }
        listBlock.Clear();
        listSpace.Clear();
    }
    public void GenerateBlock(){
        for (int i = 0; i < NumBlocks; i++){
            var length = Random.Range(MinLengthScale, MaxLengthScale);
            var space = Random.Range(MinSpace, MaxSpace);
            var B = (Transform) Instantiate(blockPrefabs);
            if (i == Math.Ceiling(NumBlocks * applyslopefreq)){
                var slope = Random.Range(-MaxSlope, MaxSlope);
                B.rotation = Quaternion.Euler(slope, B.rotation.y, B.rotation.z);
            }
            B.localScale = new Vector3(B.localScale.x, B.localScale.y, length);
            listBlock.Add(B);
            listSpace.Add(Random.Range(MinSpace, MaxSpace));
        }
        for (int i=0; i < listBlock.Count; i++){

            if (i==0){
                prevBlockZ = 0; //listBlock[i].localScale.z / 2 + listSpace[i]
                listBlock[i].position = new Vector3(0, 0, 0);
            }
            else {
                var height = Random.Range(MinHeight, MaxHeight);
                prevBlockZ += listBlock[i].localScale.z / 2 + listBlock[i-1].localScale.z / 2 + listSpace[i-1];
                listBlock[i].position = new Vector3(0, height, prevBlockZ);
            }
        }
    }

}