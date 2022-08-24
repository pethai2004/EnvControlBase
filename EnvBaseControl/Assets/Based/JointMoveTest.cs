using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class JointMoveTest : MonoBehaviour
{
    public Camera cam;
    public Agent agent;
    private JointDriveController jd_controller;
    private Transform Selected_obj;
    //this is necessary since without tracking current rotation, it will reset other unwanted axis too.
    private float currXrot=0f;
    private float currYrot=0f;
    private float currZrot=0f;

    void Start()
    {
        jd_controller = agent.GetComponent<JointDriveController>();
    }
    void FixedUpdate()
    {
        MoveOBJ();
    }
    void Update()
    {
        var obj = new List<Transform>();
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("getMouse");
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Debug.Log("HitObject");
                if (hitInfo.collider.gameObject.GetComponent<ConfigurableJoint>() != null){
                    Selected_obj = hitInfo.collider.gameObject.transform;
                    Debug.Log("hitTarget");
                }
            }
        }
    }
    public void MoveOBJ(){
        if (Selected_obj)
            {
                if (Input.GetKey("w") & Selected_obj.gameObject.GetComponent<ConfigurableJoint>().angularXMotion != ConfigurableJointMotion.Locked){
                    currXrot = 1f;
                    jd_controller.bodyPartsDict[Selected_obj].SetTarget(currXrot, currYrot, currZrot);
                    jd_controller.bodyPartsDict[Selected_obj].SetStrength(1f);
                }
                if (Input.GetKey("s") & Selected_obj.gameObject.GetComponent<ConfigurableJoint>().angularXMotion != ConfigurableJointMotion.Locked){
                    currXrot = - 1f;
                    jd_controller.bodyPartsDict[Selected_obj].SetTarget(currXrot, currYrot, currZrot);
                    jd_controller.bodyPartsDict[Selected_obj].SetStrength(1f);
                }
                if (Input.GetKey("e") & Selected_obj.gameObject.GetComponent<ConfigurableJoint>().angularYMotion != ConfigurableJointMotion.Locked){
                    currYrot = 1f;
                    jd_controller.bodyPartsDict[Selected_obj].SetTarget(currXrot, currYrot, currZrot);
                    jd_controller.bodyPartsDict[Selected_obj].SetStrength(1f);
                }
                if (Input.GetKey("d") & Selected_obj.gameObject.GetComponent<ConfigurableJoint>().angularYMotion != ConfigurableJointMotion.Locked){
                    currYrot = -1f;
                    jd_controller.bodyPartsDict[Selected_obj].SetTarget(currXrot, currYrot, currZrot);
                    jd_controller.bodyPartsDict[Selected_obj].SetStrength(1f);
                }
                if (Input.GetKey("r") & Selected_obj.gameObject.GetComponent<ConfigurableJoint>().angularZMotion != ConfigurableJointMotion.Locked){
                    currZrot = 1f;
                    jd_controller.bodyPartsDict[Selected_obj].SetTarget(currXrot, currYrot, currZrot);
                    jd_controller.bodyPartsDict[Selected_obj].SetStrength(1f);
                }
                if (Input.GetKey("f") & Selected_obj.gameObject.GetComponent<ConfigurableJoint>().angularZMotion != ConfigurableJointMotion.Locked){
                    currZrot = -1f;
                    jd_controller.bodyPartsDict[Selected_obj].SetTarget(currXrot, currYrot, currZrot);
                    jd_controller.bodyPartsDict[Selected_obj].SetStrength(1f);
                }
            }
    }
    
}
