using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ObservationCollector : MonoBehaviour
{
    public Agent agent;
    [HideInInspector] JointDriveController JDController;
    void Awake(){
        JDController = agent.GetComponent<JointDriveController>();
    }
    public void AddobservationBodyPart(VectorSensor sensor){   
        foreach (var bp in JDController.bodyPartsList){
            sensor.AddObservation(bp.rb.transform.position.normalized);
            sensor.AddObservation(bp.current_rotation.normalized);
            sensor.AddObservation(bp.current_strength / JDController.maxJointForceLimit);
            sensor.AddObservation(bp.thisJdController.GetComponent<ContractDetector>().GetCurrent());
        }
    }
    public void AddConnectedAnchor(VectorSensor sensor, List<Transform> anchor){
        foreach (var anc in anchor){
            sensor.AddObservation(anc.position.normalized);
            sensor.AddObservation(anc.rotation.normalized);
        }
    }
    public void AddAvgJointVeloc(VectorSensor sensor){
        Vector3 velSum = Vector3.zero;
        Vector3 avgVel = Vector3.zero;
        int numOfRb = 0;
        foreach (var item in JDController.bodyPartsList){
            numOfRb++;
            velSum += item.rb.velocity;
        }
        sensor.AddObservation(velSum / numOfRb);
    }
    
}
