using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BallCollisionScoreManager : MonoBehaviour
{
    [SerializeField]
    private ThrowGameSO throwGameManager;
    
    private Vector3 _initial_position;
    private Quaternion _initial_rotation;

    [SerializeField]
    public GameObject TargetActiveZone;


    private bool targetIsActive = true;
    private bool targetIsHit = false;

    List<float> HitMagnituds = new List<float>();

    private Rigidbody _rigidbody;

    [SerializeField]
    public  TextMeshPro text;

    
    private void Awake(){
        _initial_position = gameObject.transform.position;
        _initial_rotation = gameObject.transform.rotation;

        _rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        throwGameManager.RestartGameEvent.AddListener(RestartTarget);
    }

    void OnDisable()
    {
        throwGameManager.RestartGameEvent.RemoveListener(RestartTarget);
    }


    private void RestartTarget(){
        // Reset to initial position and rotation
        gameObject.transform.position = _initial_position;
        gameObject.transform.rotation = _initial_rotation;

        // Empty magnitud values
        HitMagnituds.Clear();

        //Set as active
        targetIsActive = true;
        targetIsHit = false;
    }



    // AudioSource audioSource;

    void OnCollisionEnter(Collision collision)
    {
        float magnitud = collision.relativeVelocity.sqrMagnitude;
        if(magnitud != 0 && collision.gameObject.CompareTag("GameBall")){
            HitMagnituds.Add(magnitud);
            targetIsHit = true;
        }
    }

    void Update(){
        text.SetText(targetIsActive.ToString() + " - " + _rigidbody.velocity.magnitude.ToString());

        // When Target is still active, it has been hit, and its velocity reaches zero(for the first time paired with active) -> A
        if(targetIsActive && targetIsHit && _rigidbody.velocity == Vector3.zero){

            // A -> Ensurue the item leaves the active area, mark as not active, and send the score
            //TODO Check a more representative array than just the pivot position of the target object
            if(TargetActiveZone.GetComponent<Collider>().bounds.Contains(gameObject.transform.position)){
                targetIsActive = false;
                SendScore();
            }
            
            
        }
    }

    private void SendScore(){
        int score = 0;
        foreach(float mag in HitMagnituds){
            score += (int)mag*100;
        }
        score += (int)(gameObject.transform.position.sqrMagnitude * 100);

        throwGameManager.addToScore(score);
    }


}
