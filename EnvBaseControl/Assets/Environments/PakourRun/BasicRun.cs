using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicRun : BaseEnvParkour
{
    public float destinedDistance=300f;
    public Transform blockCube;

    void Start(){
        destination.transform.position = new Vector3(0, 1, destinedDistance);
        blockCube.transform.position = new Vector3(0, 0, destinedDistance/2);
        blockCube.localScale = new Vector3(blockCube.localScale.x, blockCube.localScale.y, destinedDistance);
        GetComponentInChildren<HumanPakourAgent>().InitialDistance = destinedDistance;
    }
}
