using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;


[DefaultExecutionOrder(-5)]
public class GameManagerScript : MonoBehaviour
{
    [SerializeField]
    TextMeshPro score;

    // --------- Score variables ------------
    [SerializeField]
    private int _TotalScore = 0;

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

    public void addToScore(int amount){
        TotalScore += amount;
    }

    // --------- Cheat mode variables ------------
    [SerializeField]
    private bool _IsCheatMode = false;

    [System.NonSerialized]
    public UnityEvent<bool> OnCheatModeChange;

    public bool IsCheatMode
    {
        get { return IsCheatMode; }
        private set {  
            Physics.gravity = new Vector3(0, value? -5f: -9.81f , 0);
            _IsCheatMode = value;  
            OnCheatModeChange.Invoke(value);
             }
    }    

    public void ToggleCheatMode(){
        IsCheatMode = !IsCheatMode;
    }


    // --------- Bootstrap game ------------
    private void Start(){
        BootstrapGame(true);
    }


    //--------- Scene game states ------------
    public enum GameStateType  
    {
        Awake,
        Bootstrap,
        PlayerIdle,
        PlayerGrabbing,
        PlayerThrowing,
        FinishGame,
    }

    [System.NonSerialized]
    public UnityEvent<GameStateType> OnChangeGameState;

    private GameStateType _GameState = GameStateType.Awake;

    public GameStateType GameState
    {
        get { return _GameState; }
        set {
            _GameState = value;
            OnChangeGameState.Invoke(value);
        }
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

    private List<TargetCollisionManager> _GameTargets;

    public List<TargetCollisionManager> GameTargets {
        get {return _GameTargets; }
        private set { _GameTargets = value;}
    }

    private List<TargetCollisionManager> InitGameTargets(){
        GameTargets = new List<TargetCollisionManager>(FindObjectsOfType<TargetCollisionManager>());

        foreach(TargetCollisionManager t in  GameTargets)
            t.InitTarget();
        
        return GameTargets;
    }

    public List<TargetCollisionManager> GetActiveTargets(){
        return GameTargets.FindAll(e => e.InTargetZone);
    }

    //--------- Reference to Game Balls ------------
    private List<GameBallManager> _GameBalls;

    public List<GameBallManager> GameBalls {
        get {return _GameBalls; }
        private set { _GameBalls = value;}
    }

    private List<GameBallManager> InitGameBalls(){
        GameBalls = new List<GameBallManager>(FindObjectsOfType<GameBallManager>());
        return GameBalls;
    }

    private bool ExistActiveBalls(){
        return GameBalls.Find(e => e.ActiveBall);
    }
    
    public void BootstrapGame( bool hardBootstrap = false){
        // --- Signal bootstrap start ---
        GameState = GameStateType.Bootstrap;

        // Init and reference GameTargets
        InitGameTargets();
        InitGameBalls();

        // Reseting common game variables
        TotalScore = 0;
    }


    private void FixUpdate(){
        // 
        if(!ExistActiveBalls())
            CountFinalScore();
        
    }

    private void CountFinalScore(){
        int score = 0;
        foreach (TargetCollisionManager gt in GameTargets)
            score += gt.CalculateTargetScore();
        
        TotalScore = score;

        GameState = GameStateType.FinishGame;
    }

}
