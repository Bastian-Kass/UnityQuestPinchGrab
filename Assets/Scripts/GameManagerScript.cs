using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;


// Script has run priority
[DefaultExecutionOrder(-5)]
public class GameManagerScript : MonoBehaviour
{

    public TextMeshPro debug_text;

    // --------- Score variables ------------
    [SerializeField, HideInInspector]
    private int _TotalScore = 0;

    [HideInInspector]
    public int TotalScore
    {
        get { return _TotalScore; }
        private set { 
            _TotalScore = value;  
            OnScoreChange.Invoke(value);
            }
    }    
    
    [System.NonSerialized]
    public UnityEvent<int> OnScoreChange;

    // ---- Audio source management ----
    [SerializeField]
    public AudioSource winSoundAudio;
    [SerializeField]
    public AudioSource loseSoundAudio;

    public void addToScore(int amount){
        TotalScore += amount;
    }

    // --------- Bootstrap game ------------
    private void Start(){
        BootstrapGame();
    }

    private void FixedUpdate(){
        // Checking for active balls -> Game finishes when there are none
        // TODO: Check efficiency of checking this way, making balls invoke an event and counting the events to mark a finished game
        if(GameState == GameStateType.PlayerThrowing && (AreAllTargetsHit() || AreAllBallsInactive()))
            CountFinalScore();
    }

    //--------- Scene game states ------------
    public enum GameStateType  
    {
        Awake,
        Bootstrap,
        PlayerThrowing,
        FinishGame,
    }

    [System.NonSerialized]
    public UnityEvent<GameStateType> OnChangeGameState;

    private GameStateType _GameState = GameStateType.Awake;

    public GameStateType GameState
    {
        get { return _GameState; }
        set { _GameState = value; OnChangeGameState.Invoke(value); }
    }

    private void OnEnable (){
        if (OnChangeGameState == null)
            OnChangeGameState = new UnityEvent<GameStateType>();

        if (OnScoreChange == null)
            OnScoreChange = new UnityEvent<int>();

        if (OnCheatModeChange == null)
            OnCheatModeChange = new UnityEvent<bool>();
    }

    //--------- Reference to Gametargets ------------
    public List<TargetCollisionManager> GameTargets { get; private set; }

    private List<int> HitScoresList = new List<int>();

    private void InitGameTargets(){

        // Clearing previous score list
        HitScoresList.Clear();

        // Removing previous listeners, if any
        if(GameTargets != null && GameTargets.Count > 0)
            foreach(TargetCollisionManager t in  GameTargets)
                t.OnTargetHit.RemoveListener(OnTargetBeingHit);
                
        // Referencing game targets and adding listener to each
        GameTargets = new List<TargetCollisionManager>(FindObjectsOfType<TargetCollisionManager>());

        foreach(TargetCollisionManager t in  GameTargets){
            t.InitTarget();
            if(t.OnTargetHit != null)
                t.OnTargetHit.AddListener(OnTargetBeingHit);
        }
    }

    private void OnTargetBeingHit(Collision collision){

        //Calculating the Hit Score
        int mag = (int)(collision.relativeVelocity.sqrMagnitude * 20);
        HitScoresList.Add(mag);

        addToScore(mag);
    }

    private bool AreAllTargetsHit(){
        return (GameTargets.FindIndex(e => e.InTargetZone) == -1);
    }

    //--------- Reference to Game Balls ------------
    public List<GameBallManager> GameBalls { get; private set; }

    private List<GameBallManager> InitGameBalls(){

        //Other logic that might come in handy when initializing the game throwing objects
        // ...

        GameBalls = new List<GameBallManager>(FindObjectsOfType<GameBallManager>());
        return GameBalls;
    }

    private bool AreAllBallsInactive(){
        return (GameBalls.FindIndex(e => e.ActiveBall) == -1);
    }


    // ------- General Functions ------
    public void BootstrapGame(){
        // --- Signal bootstrap start ---
        GameState = GameStateType.Bootstrap;

        // Init and reference GameTargets
        InitGameTargets();
        InitGameBalls();

        // Reseting common game variables
        TotalScore = 0;
        HitScoresList.Clear();

        // Following the game state logic
        GameState = GameStateType.PlayerThrowing;
    }

    private void CountFinalScore(){
        int score = 0;
        // Adding to the score the distance of each can
        foreach (TargetCollisionManager gt in GameTargets)
            score += gt.CalculateTargetScore();

        // Bonus for balls not used!
        foreach( GameBallManager gb in GameBalls){
            if(!gb.ThrownBall)
                score += 5000;
        }

        // Adding to score for each hit and how powerfull it was (magnitud)
        foreach( int n in HitScoresList)
            score += n;
        
        // Setting the final score!
        TotalScore = score;

        if(score != 0)
            winSoundAudio.Play();
        else    
            loseSoundAudio.Play();

        GameState = GameStateType.FinishGame;
    }


    // --------- Cheat mode variables ------------

    [SerializeField, HideInInspector]
    private bool _IsCheatMode = false;
    [SerializeField, Range(-5f, -9.81f)]
    public float CheatGravity = -9.81f;

    [System.NonSerialized]
    public UnityEvent<bool> OnCheatModeChange;

    public bool IsCheatMode
    {
        get { return _IsCheatMode; }
        private set {  
            Physics.gravity = new Vector3(0, CheatGravity , 0);
            _IsCheatMode = value; 
            OnCheatModeChange.Invoke(_IsCheatMode);
            }
    }    

    public void ToggleCheatMode(){
        IsCheatMode = !IsCheatMode;
    }




}
