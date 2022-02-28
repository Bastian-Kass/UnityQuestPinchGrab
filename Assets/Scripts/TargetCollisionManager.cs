using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Oculus.Interaction;
public class TargetCollisionManager : MonoBehaviour
{
    [SerializeField]
    public GameManagerScript throwGameManager;

    [SerializeField]
    public AudioTrigger triggerScript;

    [System.NonSerialized]
    public UnityEvent<int> OnTargetHit;

    public Rigidbody _rigidbody { get; private set; }

    [SerializeField]
    public Renderer _renderer;
    private Material _material;

    private Color _active_color;
    private Color _inactive_color;

    private Vector3 _initial_position;
    private Quaternion _initial_rotation;

    private List<float> HitMagnituds = new List<float>();

    private bool _InTargetZone = true;
    
    public bool InTargetZone {
        get {return _InTargetZone; }
        private set { _InTargetZone = value; }
    }

    void OnEnable()
    {
        if (OnTargetHit == null)
            OnTargetHit = new UnityEvent<int>();
    }

    private void Awake(){
        _initial_position = gameObject.transform.position;
        _initial_rotation = gameObject.transform.rotation;

        _rigidbody = gameObject.GetComponent<Rigidbody>();

        _active_color = new Color(1f,0f,0f, 1f);
        _inactive_color = new Color(.4f,.4f,.4f, 1f);

        _material = _renderer.materials[1];
    }

    public void InitTarget(){
        _material.color = _active_color;
        //Removing velicty when positioning
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        // Reset to initial position and rotation
        gameObject.transform.position = _initial_position;
        gameObject.transform.rotation = _initial_rotation;

        //Set as active
        InTargetZone = true;

        //Cumulative score reset
        HitMagnituds.Clear();
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("TargetZone")){
            InTargetZone = false;
            _material.color = _inactive_color;
        } 
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


                SetDebug(magnitud.ToString());
            }

        }

        // Need more sound effects to randomize
        if(triggerScript != null && collision.relativeVelocity.sqrMagnitude >= 2)
            triggerScript.PlayAudio();
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
