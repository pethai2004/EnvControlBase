using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContractDetector : MonoBehaviour
{
    [HideInInspector] public bool contractedground=false;
    public string GroundContract="ground";
    // [HideInInspector] public bool contractedtarget=false;
    // public string SpecificTarg="TargetObject";

    void OnCollisionEnter(Collision other) {
        if (other.transform.CompareTag(GroundContract)){
            contractedground=true;
        }
        // if (SpecificTarg != null){
        //     SpecifyTarget(other);
        // }
    }
    void OnCollisionExit(Collision other) {
        if (other.transform.CompareTag(GroundContract)){
            contractedground=false;
        }
        // if (SpecificTarg != null){
        //     SpecifyTarget(other);
        // }
    }
    public float GetCurrent(){
        if (contractedground){return 1f;}
        else {return 0f;}
    }
    // public void SpecifyTarget(Collision other){
    //     if (other.transform.CompareTag(SpecificTarg)){
    //         contractedtarget=true;
    //     }
    //     else {
    //         contractedtarget=false;
    //         }
    // }
}
