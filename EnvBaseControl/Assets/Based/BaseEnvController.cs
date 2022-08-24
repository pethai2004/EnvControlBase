using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnvController : MonoBehaviour
{
    public float TimeScale=2f;
    private float fixedDeltaTime;

    void Awake() {
        this.fixedDeltaTime = Time.fixedDeltaTime;
        Time.timeScale = TimeScale;
        Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
    }
    
}
