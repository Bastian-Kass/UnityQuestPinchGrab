using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DebugManager : MonoBehaviour
{

    public GameObject debugBall;

    public bool debugMode = false;

    public bool SlowMode { get; private set; }

    public Vector3 ThrowPosition;
    public Vector3 ThrowDirection;
    private float TimeStep = 0.03f;

    public float slowdownFactor = 0.05f;

    public void OnEnable(){
        this.SlowMode = false;
        this.TimeStep = Time.fixedDeltaTime;
    }

    public void ToggleSlowMotion(){
        if(SlowMode){
            Time.timeScale = 1f;
            Time.fixedDeltaTime = this.TimeStep ;

            SlowMode = false;
        } else{
            Time.timeScale = this.slowdownFactor;
            Time.fixedDeltaTime = Time.timeScale * this.TimeStep;

            SlowMode = true;
        }
    }


    public void ThrowDebugBall(){
        debugBall.GetComponent<GameBallManager>().Initialize();
        debugBall.SetActive(false);
        debugBall.transform.position = ThrowPosition;
        debugBall.SetActive(true);
        debugBall.GetComponent<Rigidbody>().velocity = Vector3.zero;
        debugBall.GetComponent<Rigidbody>().AddForce(ThrowDirection, ForceMode.VelocityChange);
        
    }

    private void OnDrawGizmos(){
        Gizmos.color = Color.white;
        Gizmos.DrawLine(ThrowPosition, ThrowDirection);
    }
}
