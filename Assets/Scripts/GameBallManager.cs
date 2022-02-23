using UnityEngine;
// using Oculus.Interaction.HandPosing;
using TMPro;
using System.Collections;

public class GameBallManager : MonoBehaviour
{
    public Rigidbody _rigidbody;
    private Vector3 _initial_position;
    private Quaternion _initial_rotation;

    [SerializeField]
    private GameManagerScript throwGameManager;

    // Maybe will need at certain point
    // public HandGrabInteractable handGrabInteractable;


    public bool ActiveBall = true;
    public bool ThrownBall = false;

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
            ThrownBall = true;
        }

        if(other.CompareTag("GameBounds")){
            StartCoroutine(BallOutOfBounds());
        }
    }

    IEnumerator BallOutOfBounds(){
        while (true)
        {
            yield return new WaitForSeconds(3);
            //TODO: Might want to show the ball as grey
            ActiveBall = false; 
        }
    }


    private void FixedUpdate(){
        // When an active and thrown ball stops moving: Desactivate it and calculate score
        if(ActiveBall && ThrownBall && throwGameManager.IsCheatMode){

            TargetCollisionManager[] targets = FindObjectsOfType<TargetCollisionManager>();

            foreach (TargetCollisionManager t in targets){
                if(t.InTargetZone){
                    Attract(t);
                }
            }

            
        }

        

    }

    void Attract (TargetCollisionManager rigidbodyToAttract){

        Vector3 direction = _rigidbody.position - rigidbodyToAttract._rigidbody.position;
        // float distance = direction.sqrMagnitude;

        //  ------ Simplification -----
        // Determine the face gravitation by changing the mass * mass on the gravitational formula
        // No need to determine magnitud of the direction and then squareing it, sqrMagnitud already does that

        float forceMagnitud = 3 / direction.sqrMagnitude; //Some simplification
        //  ------ Simplification -----
        Vector3 force = direction.normalized * forceMagnitud;

        rigidbodyToAttract._rigidbody.AddForce(force);

    }

    // ---- Debug text output ---
    
    [SerializeField]
    public  TextMeshPro text_debug;

}
