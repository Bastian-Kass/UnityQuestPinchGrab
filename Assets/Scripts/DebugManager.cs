using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DebugManager : MonoBehaviour
{

    public GameObject debugBall;

    private bool debugMode = false;

    public Vector3 ThrowPosition;
    public Vector3 ThrowDirection;
    private LineRenderer LineDrawer;

    public void OnEnable(){
        LineDrawer = gameObject.GetComponent<LineRenderer>();
    }

    public void NextPhysicsFrame(){

        if(debugMode){
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 1f;

            debugMode = false;
        } else{

            Time.timeScale = .05f;
            Time.fixedDeltaTime = Time.timeScale * .02f;

            debugMode = true;
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

    public void Update(){
        LineDrawer.SetPosition(0, ThrowPosition);
        LineDrawer.SetPosition(1, ThrowDirection);
    }
}
