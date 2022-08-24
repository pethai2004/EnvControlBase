using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class TargetDs : MonoBehaviour
{

    public string ContractName="agent";
    public bool Contracted=false;
    public bool returnAgent=false;
    [HideInInspector] public GameObject ag;

    private void OnCollisionEnter(Collision other) {
        if (other.transform.CompareTag(ContractName)){
            Contracted=true;
            if (returnAgent==true){
                ag = other.gameObject;
            }
        }
    }
    private void OnCollisionExit(Collision other) {
        if (other.transform.CompareTag(ContractName)){
            Contracted=false;
        }
    }
}
