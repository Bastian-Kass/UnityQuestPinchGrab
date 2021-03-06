using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;

public class GameBallManager : MonoBehaviour
{

    [SerializeField]
    private GameManagerScript GameManager;

    public Rigidbody _rigidbody;
    private Vector3 _initial_position;
    private Quaternion _initial_rotation;
    private Material _material;
    private bool _ActiveBall = false;
    public bool ActiveBall
       {
       get { return _ActiveBall; }
       set 
       {
           _ActiveBall = value;
           if(_material != null){
                _material.color = value? Color.white: Color.black;
           }
                
       }
   }
    public bool ThrownBall { get; private set; }

    // Cheat mode physics variables
    private List<TargetCollisionManager> _targets;
    private Vector3 _centerOfMass;
    private Vector3 Orthonormal_to_direction;

    [SerializeField, Range(0, 2)]
    public float CenterOfMassDistance = 0.75f;

    //Audio variables
    [SerializeField]
    public AudioTrigger triggerScript_flyingball;
    [SerializeField, Range(0, 10)]
    public float AudioCollision_Threshold = 2;
    [SerializeField]
    public AudioTrigger triggerScript_collision;

    // Debug variables
    public GameObject CenterOfMassRep;

    void OnEnable()
    {
        GameManager.OnChangeGameState.AddListener(RestartPosition);
        // handGrabInteractable.OnPointerEvent += OnHandGrabEvent;
    }

    void OnDisable()
    {
        GameManager.OnChangeGameState.RemoveListener(RestartPosition);
        // handGrabInteractable.OnPointerEvent -= OnHandGrabEvent;
    }

    private void Awake()
    {

        //Reference to rigidbody for collision velocity calculations
        _rigidbody = gameObject.GetComponent<Rigidbody>();

        // Initial position and rotation for reset
        _initial_position = gameObject.transform.position;
        _initial_rotation = gameObject.transform.rotation;

        Orthonormal_to_direction = new Vector3();
        _material = gameObject.GetComponentInChildren<Renderer>().material;

        _targets = new List<TargetCollisionManager>();
    }

    private void FixedUpdate()
    {
        // When in cheatmode: a thrown ball will be constantly attracted
        if(ActiveBall && ThrownBall && GameManager.IsCheatMode)
            AttractBallToTarget();      
    }

    public void Initialize()
    {
        //Making it not move when reseting!!
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        //Reseting the object to the initial position
        gameObject.transform.position = _initial_position;
        gameObject.transform.rotation = _initial_rotation;
        
        //Setting as active
        ActiveBall = true;
        ThrownBall = false;
        outOfBoundsCounter_Flag = false;
    }

    private void RestartPosition(GameManagerScript.GameStateType state)
    {
        // On game signaling Bootstrap, Initializing target to its original values
        if(state == GameManagerScript.GameStateType.Bootstrap)
            Initialize();
    }

    void OnCollisionStay(Collision collision)
    {
         
        //Active balls that have not been thrown, respawn after staying collisioning 
        // with the wooden pallet below
        if(ActiveBall && !ThrownBall && collision.gameObject.CompareTag("Respawn"))
            Initialize();


        // TODO: Investigate -> efficiency of OnTriggerExit( defined Gamebounds ) VS OnCollisionStay( with any object other than the defined ones [respawn, hands, other balls] )
         
    }

    void OnCollisionEnter(Collision collision)
    {
        //TODO: Determine a proper magnitude to signal colission sound (ping pong ball sound)
        if(triggerScript_collision != null && collision.relativeVelocity.sqrMagnitude > AudioCollision_Threshold)
            triggerScript_collision.PlayAudio();
    }

    void OnTriggerExit(Collider other)
    {
        // Manage when ball leaves the throwing zone
        if(other.CompareTag("ThrowingZone")){

            if(GameManager.IsCheatMode)
                CalculateCheatModeSettings();

            // Ball considered thrown when leaving the throwing zone
            // TODO: Consider if the player is not holding the ball
            ThrownBall = true;

            // Pragmatically, we can trigger the effect sound of the ball swishing when it leaves the throw-area
            if(triggerScript_flyingball != null)
                triggerScript_flyingball.PlayAudio();
        }

        // Manage when balls leaves the playing bounds
        if(other.CompareTag("GameBounds"))
            StartCoroutine(BallOutOfBounds(other));
        
    }

    private void CalculateCheatModeSettings()
    {
            // Getting all active targets
            TargetCollisionManager[] targets = FindObjectsOfType<TargetCollisionManager>();

            _targets.Clear();

            foreach ( var target in targets)
                if(target.InTargetZone)
                    _targets.Add(target);

            if(_targets.Count == 0)
                return;

            // We are only interested in the center of mass
            _centerOfMass = CalculateTargetCenterOfMass();

            // Moving the center of mass to a between point between the ball and the targets.
            _centerOfMass = Vector3.Lerp(_centerOfMass, _rigidbody.position, 0.3f);


            CenterOfMassRep.SetActive(true);
            CenterOfMassRep.GetComponent<Rigidbody>().position = _centerOfMass;

            // Ignoring gravity; only interested in direction on the plane x,z
            Vector3 ballDirection = _rigidbody.velocity;
            ballDirection.y = 0;

            // Cross product of x&z velocity components with the up vector returns the orthonormal vector pointing right to the ball throw
            Orthonormal_to_direction = Vector3.Cross(ballDirection, Vector3.up ).normalized;
                  
    }

    void AttractBallToTarget ()
    {

        if(_targets.Count == 0)
            return;

        // Getting the distance vector of the ball and the center of mass
        Vector3 distance_vector = _rigidbody.position - _centerOfMass;

        // Being carefull not to device by cero (even when highly improbable)
        float pull_magnitude = GameManager.CheatModePower / (distance_vector.sqrMagnitude + 0.01f);

        //Creating the force accordingly
        Vector3 CheatModeForce = Orthonormal_to_direction * pull_magnitude * IsRightFromDirection(_centerOfMass);

        // Finally adding the force to the object
        _rigidbody.AddForce(CheatModeForce);

    }

    private float IsRightFromDirection(Vector3 _centerOfMass){
        float angle = Vector3.SignedAngle(
                            new Vector3(_rigidbody.position.x + _rigidbody.velocity.x, 0, _rigidbody.position.z + _rigidbody.velocity.z), 
                            new Vector3(_centerOfMass.x, 0, _centerOfMass.z),
                            new Vector3(_rigidbody.position.x, 0 ,_rigidbody.position.z)
                            );

        return Mathf.Sign(angle);
    }

    private Vector3 CalculateTargetCenterOfMass()
    {
            Vector3 sum_vector = new Vector3(0,0,0);

            foreach (TargetCollisionManager t in _targets)
                sum_vector += t.GetRigidbody().position;

            return sum_vector/_targets.Count;
    }

    private bool outOfBoundsCounter_Flag = false;


    /// <summary>
    /// Counter to determine if ball has left and stayed out of bounds for certain amount of time
    /// </summary>
    IEnumerator BallOutOfBounds(Collider other)
    {

        outOfBoundsCounter_Flag = true;

        yield return new WaitForSeconds(1);


        // Recheck if the ball is still out of bounds.
        if(other.bounds.Contains(transform.position))
            yield return null;

        //After countdown we deactivate the ball
        if(outOfBoundsCounter_Flag)
            ActiveBall = false; 

        outOfBoundsCounter_Flag = false;
        
    }


    //  ----- Not useful right now, I just dont want to lose the code to detect grab events ---
    // Maybe will need at certain point
    // public HandGrabInteractable handGrabInteractable;


    // private void OnHandGrabEvent(PointerArgs args){
    //     //Signaling the game that a ball has been grabbed
    //     // if(args.PointerEvent == PointerEvent.Select){
    //     //     //Getting targets inside the active zone to know which hits count
    //     //     throwGameManager.GameState = GameManagerScript.GameStateType.PlayerGrabbing;

    //     //     // //TODO: Define user area, once ball leaves it is no longer active [Instead of deactivating when grabbed]
    //     //     // ActiveBall = false;
    //     // }

    //     // //Signaling the game that a ball has been released
    //     // if(args.PointerEvent == PointerEvent.Unselect){
    //     //     throwGameManager.GameState = GameManagerScript.GameStateType.PlayerThrowing;
    //     // }


    // }

}
