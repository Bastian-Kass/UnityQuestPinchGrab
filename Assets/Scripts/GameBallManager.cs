using UnityEngine;
using System.Collections;
using Oculus.Interaction;

public class GameBallManager : MonoBehaviour
{

    [SerializeField]
    private GameManagerScript GameManager;


    public Rigidbody _rigidbody;
    private Vector3 _initial_position;
    private Quaternion _initial_rotation;

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

    [SerializeField, Range(5, 10)]
    public float CheatModePower = 5;
    private Vector3 Ortonormal_to_direction;



    [SerializeField]
    public AudioTrigger triggerScript_flyingball;

    [SerializeField, Range(0, 10)]
    public float AudioCollision_Threshold = 2;
    [SerializeField]
    public AudioTrigger triggerScript_collision;

    private Material _material;

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

    private void Awake(){

        //Reference to rigidbody for collision velocity calculations
        _rigidbody = gameObject.GetComponent<Rigidbody>();

        // Initial position and rotation for reset
        _initial_position = gameObject.transform.position;
        _initial_rotation = gameObject.transform.rotation;

        Ortonormal_to_direction = new Vector3();
        _material = gameObject.GetComponentInChildren<Renderer>().material;
    }

    public void Initialize(){
        //Making it not move when reseting!!
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        //Reseting the object to the initial position
        gameObject.transform.position = _initial_position;
        gameObject.transform.rotation = _initial_rotation;
        
        //Setting as active
        ActiveBall = true;
        ThrownBall = false;
    }


    private void RestartPosition(GameManagerScript.GameStateType state){
        // On game signaling Bootstrap, Initializing target to its original values
        if(state == GameManagerScript.GameStateType.Bootstrap)
            Initialize();
    }



    void OnCollisionStay(Collision collision)
    {
         
        //Active balls that have not been thrown, respawn after staying collisioning 
        // with the wooden palet below
        if(ActiveBall && !ThrownBall && collision.gameObject.CompareTag("Respawn"))
            Initialize();


        // TODO: Investigate -> efficiency of OnTriggerExit( defined Gamebounds ) VS OnCollisionStay( with any object other tahn the defined ones [respawn, hands, other balls] )
         
    }

    void OnCollisionEnter(Collision collision){
        //TODO: Determine a proper magnitud to signal colission sound (ping pong ball sound)
        if(triggerScript_collision != null && collision.relativeVelocity.sqrMagnitude > AudioCollision_Threshold)
            triggerScript_collision.PlayAudio();
    }

    void OnTriggerExit(Collider other)
    {
        //Ball flagged as thrown when it leaves the throwing area
        if(other.CompareTag("ThrowingZone")){
            /*
                Calculating the orthonormal vector of the thrown ball (direction x gravity)
                This will be used in the cheatmode since the ball will only correct direction in that axis:
                  YES > The ball WILL  apply a force in this perpendicular vector
                  NO  > The ball will not accelerate towards the target in in front/back of it
                  NO  > The ball will not accelerate towards the target below above it
                - Only the initial orthonmal vector is neded ( this is not calculated continualy in the update method )

            */
            //Obtaining the orthonormal vector orthonormal from gravity (up/down) and the initial direction in the x.z plane axis [we use the velocity on x,y of the initial velocity]
            Ortonormal_to_direction = Vector3.Cross(new Vector3( _rigidbody.velocity.x, 0, _rigidbody.velocity.z), Physics.gravity).normalized;
            ThrownBall = true; 
        
            // Pragmatically, we can trigger the effect sound of the ball swishing when it leaves the throw-area
            if(triggerScript_flyingball != null)
                triggerScript_flyingball.PlayAudio();
        }

        // Detect if the ball left the play-area, in which case, after x seconds, will be flagged as Inactive [ActiveBall = false]
        if(other.CompareTag("GameBounds")){
            StartCoroutine(BallOutOfBounds(other));
        }
    }

    IEnumerator BallOutOfBounds(Collider other){
        yield return new WaitForSeconds(1);
        //After the one minute wait, we only change it to inactive if it is still out of the zone
        // [Maybe the user restarts the game in between this time]
        if(!other.bounds.Contains(transform.position))
            ActiveBall = false; 
        
    }


    private void FixedUpdate(){

        // When in cheatmode: a thown ball will be constantly attracted
        if(ActiveBall && ThrownBall && GameManager.IsCheatMode){


            //Get targets in the game
            TargetCollisionManager[] targets = FindObjectsOfType<TargetCollisionManager>();

            Vector3 sum_vector = new Vector3(0,0,0);
            int vector_count = 0;

            //Proceed to calculate a mean position of all targets [Will work as center of mass ]
            foreach (TargetCollisionManager t in targets)
                if(t.InTargetZone){
                    vector_count ++;
                    sum_vector += t.GetRigidbody().position;
                }
                    
            // Attract the gameball to the calculated center of mass
            if(vector_count > 0)
                Attract(sum_vector/vector_count);
            
        }
    }


    void Attract (Vector3 meanTargetPosition){

        // TODO:  -----    IMPORTANT  -------
        /* There needs to be a maximum a constant velocity!!, 
        /* we cannot only add the force or the ball accelarates too much and sends the cans flying like crazy
        */ 


        /* NOTE: This calculation is a rough simplification of the gravity formula
            m1, m2 and gravity constant are swapped for a constant (cheat-mode-power) to have control over the attraction
            since the formula determines the distance^2, we can directly know it by the sqrMagnitud of the distance vector
            We finalize by adding the projected force into the orthonormal vector calculated on throw, and times the forceMagnitud
        */
        //TODO: Add graph explanation of these maths

        // Distance vector of the center of mass of all the cans (mean position of the targets)
        Vector3 distance = _rigidbody.position - meanTargetPosition;

        float forceMagnitud = CheatModePower / distance.sqrMagnitude;

        _rigidbody.AddForce(  Vector3.Project(distance.normalized, Ortonormal_to_direction) * - forceMagnitud );

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
