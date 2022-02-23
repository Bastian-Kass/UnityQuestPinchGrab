using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class TargetCollisionManager : MonoBehaviour
{
    [SerializeField]
    public GameManagerScript throwGameManager;

    public Rigidbody _rigidbody;
    private Vector3 _initial_position;
    private Quaternion _initial_rotation;

    private List<float> HitMagnituds = new List<float>();

    private bool _InTargetZone = true;
    
    public bool InTargetZone {
        get {return _InTargetZone; }
        private set { _InTargetZone = value; }
    }

    // void OnEnable()
    // {
    //     throwGameManager.OnChangeGameState.AddListener(RestartPosition);
    // }

    // void OnDisable()
    // {
    //     throwGameManager.OnChangeGameState.RemoveListener(RestartPosition);
    // }

    // private void RestartPosition(GameManagerScript.GameStateType state){
    //     // On game signaling Bootstrap, Initializing target to its original values
    //     if(state == GameManagerScript.GameStateType.Bootstrap)
    //         InitTarget();
    // }

    
    private void Awake(){
        _initial_position = gameObject.transform.position;
        _initial_rotation = gameObject.transform.rotation;

        _rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    public void InitTarget(){
        // Reset to initial position and rotation
        gameObject.transform.position = _initial_position;
        gameObject.transform.rotation = _initial_rotation;

        //Set as active
        InTargetZone = true;

        //Cumulative score reset
        HitMagnituds.Clear();
    }


    private void onTriggerExit(Collider other){
        if(other.CompareTag("TargetZone"))
            InTargetZone = false;
    }   



    void OnCollisionEnter(Collision collision)
    {

        // An active-thrown ball that collisions with a GameTarget adds to the score-list
        if(collision.gameObject.CompareTag("GameBall") && InTargetZone){

            //TODO: Draw some sort of "Pow" Onomatopeya

            //TODO: Play sounds [Empty can when not InTargetZone, better sound when InTargetZone]

            GameBallManager ball = collision.gameObject.GetComponent<GameBallManager>();

            if(ball.ActiveBall){
                float magnitud = collision.relativeVelocity.sqrMagnitude;
                HitMagnituds.Add(magnitud);

                // Temporary score just to show change to the user -> final one is recalculated at the end
                throwGameManager.addToScore((int)magnitud);
            }

        }
    }

    public int CalculateTargetScore(){
        int score = 0;
        foreach (float m in HitMagnituds)
            score += (int)(m * 1000);

        score += (int)( ( transform.position - _initial_position).sqrMagnitude * 20);
        return score;
    }

    // --- Debug Text Gameobject ---

    [SerializeField]
    public  TextMeshPro text;

    public void SetDebug(string s){
        text.SetText(s);
    }

}
