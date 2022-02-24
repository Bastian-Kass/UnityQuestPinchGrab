using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class GameBallManager : MonoBehaviour
{
    public Rigidbody _rigidbody;
    public Vector3 _initial_position;
    private Quaternion _initial_rotation;

    [SerializeField]
    private GameManagerScript throwGameManager;

    // Maybe will need at certain point
    // public HandGrabInteractable handGrabInteractable;


    public bool ActiveBall = true;
    public bool ThrownBall = false;

    private Vector3 Ortonormal_to_direction;

    void OnEnable()
    {
        throwGameManager.OnChangeGameState.AddListener(RestartPosition);
        // handGrabInteractable.OnPointerEvent += OnHandGrabEvent;
    }

    void OnDisable()
    {
        throwGameManager.OnChangeGameState.RemoveListener(RestartPosition);
        // handGrabInteractable.OnPointerEvent -= OnHandGrabEvent;
    }

    private void Awake(){

        //Reference to rigidbody for collision velocity calculations
        _rigidbody = gameObject.GetComponent<Rigidbody>();

        // Initial position and rotation for reset
        _initial_position = gameObject.transform.position;
        _initial_rotation = gameObject.transform.rotation;

        Ortonormal_to_direction = new Vector3();
    }

    public void InitTarget(){
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
            InitTarget();
    }

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

    void OnTriggerExit(Collider other)
    {
        //Ball is flagges as thrown when it leaves the throwing area
        if(other.CompareTag("ThrowingZone")){
            //Obtaining the orthonormal vector to the initial velocity and the gravity on player's throw
            Ortonormal_to_direction = Vector3.Cross(new Vector3( _rigidbody.velocity.x, 0, _rigidbody.velocity.z), Physics.gravity).normalized;
            Debug.Log(Ortonormal_to_direction.ToString());
            ThrownBall = true; 
        }

        if(other.CompareTag("GameBounds")){
            StartCoroutine(BallOutOfBounds());
        }
    }

    IEnumerator BallOutOfBounds(){
        yield return new WaitForSeconds(3);
        //TODO: Might want to show the ball as grey
        ActiveBall = false; 
    }


    private void FixedUpdate(){
        var keybooard = Keyboard.current;
        if (keybooard == null)
            return; // No gamepad connected.

        if (keybooard.spaceKey.wasPressedThisFrame)
        {   
            _rigidbody.velocity = new Vector3 (1f, 20f, -2f);
        }

        // When an active and thrown ball stops moving: Desactivate it and calculate score
        if(ActiveBall && ThrownBall && throwGameManager.IsCheatMode){

            TargetCollisionManager[] targets = FindObjectsOfType<TargetCollisionManager>();
            
            Vector3 sum_vector = new Vector3(0,0,0);
            int vector_count = 0;

            foreach (TargetCollisionManager t in targets)
                if(t.InTargetZone){
                    vector_count ++;
                    sum_vector += t._rigidbody.position;
                }
                    
            if(vector_count > 0)
                Attract(sum_vector/vector_count);
            
        }
    }


    void Attract (Vector3 meanTargetPosition){

        // Distance vector of the center of mass of all the cans (mean position of the targets)
        Vector3 distance = _rigidbody.position - meanTargetPosition;

        // //  ------ Simplification -----
        // // Based on the gravitational formula, where mass of the objects is constant* and we should know the constant of gravity
        // //  The closer the object, the stronger the pull is: Dividing the force by the distance

        

        float forceMagnitud = 3 / distance.sqrMagnitude;

        _rigidbody.AddForce(Ortonormal_to_direction * forceMagnitud);

    }

    // ---- Debug text output ---
    
    [SerializeField]
    public  TextMeshPro text_debug;


}
