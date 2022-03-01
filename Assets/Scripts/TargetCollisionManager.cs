
using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;
public class TargetCollisionManager : MonoBehaviour
{

    [System.NonSerialized]
    public UnityEvent<Collision> OnTargetHit;

    public bool InTargetZone { get; private set; }

    private Rigidbody _rigidbody;
    private Vector3 _initial_position;
    private Quaternion _initial_rotation;

    [Header("Visual feedback settings")]
    [SerializeField]
    public Renderer _renderer;
    private Material _material;
    private Color _active_color;
    private Color _inactive_color;
    public int ActiveMaterialPosition = 2;

    [Header("Audio on collision settings")]
    [SerializeField]
    public AudioTrigger AudioTriggerScript;
    [SerializeField, Range(0, 10)]
    public float AudioCollision_Threshold = 2;

    [Header("Game score settings")]
    [SerializeField]
    public int ScoreMultiplier_Distance = 20;

    void OnEnable()
    {
        if (OnTargetHit == null)
            OnTargetHit = new UnityEvent<Collision>();
    }

    private void Awake(){
        // Getting variables from the target's start position
        _rigidbody = gameObject.GetComponent<Rigidbody>();

        _initial_position = gameObject.transform.position;
        _initial_rotation = gameObject.transform.rotation;

        // TODO: We would be able to set these public and change them on editor
        _active_color = new Color(1f,0f,0f, 1f);
        _inactive_color = new Color(.4f,.4f,.4f, 1f);

        // NOTE: material at position 1 is the red material on the can target
        Material[] materials = _renderer.materials;

        //Just making sure nothing break if material does not exist
        if(materials.Length >= ActiveMaterialPosition)
            _material = materials[ActiveMaterialPosition-1];
        else if(materials.Length >= 1)
            _material = materials[0];
    }

    public void InitTarget(){
        //Removing velicty when positioning
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        // Reset to initial position and rotation
        gameObject.transform.position = _initial_position;
        gameObject.transform.rotation = _initial_rotation;

        //Set as active
        InTargetZone = true;

        // Set Feedback color
        _material.color = _active_color;
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("TargetZone")){
            InTargetZone = false;
            //Coloring the target as inactive
            _material.color = _inactive_color;
        } 
    }

    void OnCollisionEnter(Collision collision)
    {

        // An active-thrown ball that collisions with a GameTarget adds to the score-list
        if(collision.gameObject.CompareTag("GameBall") && InTargetZone){


            // Getting the collision, and only triggering if the ball is active
            // [Eg. Ignoring balls on the ground that get hit by a falling can]
            GameBallManager ball = collision.gameObject.GetComponent<GameBallManager>();

            if(ball.ActiveBall)
                OnTargetHit.Invoke(collision);
            

        }

        // Need more sound effects to randomize
        // TODO: Get the propoer value programatically for the audio to trigger, now using 2
        // TODO: Get more audios randomze and add varity
        if(AudioTriggerScript != null && collision.relativeVelocity.sqrMagnitude >= AudioCollision_Threshold)
            AudioTriggerScript.PlayAudio();
    }

    public int CalculateTargetScore(){
        int score = 0;

        score += (int)( ( transform.position - _initial_position).sqrMagnitude * ScoreMultiplier_Distance);
        return score;
    }

    // --------- Misc ------------
    public Rigidbody GetRigidbody (){
        return _rigidbody;
    }


}
