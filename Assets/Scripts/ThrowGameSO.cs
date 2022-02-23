using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Threading;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "GameData", menuName = "ScriptableObjects/ThrowGameSO", order = 1)]
public class ThrowGameSO : ScriptableObject
{
    //--------- Score variables ------------
    [SerializeField]
    private int _TotalScore = 0;
    [System.NonSerialized]
    public UnityEvent<int> OnScoreChange;

    private int TotalScore
    {
        get { return _TotalScore; }
        set {  _TotalScore = value;  OnScoreChange.Invoke(value); }
    }    
    
    public void addToScore(int amount){
        TotalScore += amount;
    }

    //--------- Cheat mode variables ------------

    [SerializeField]
    private bool _IsCheatMode = false;

    [System.NonSerialized]
    public UnityEvent<bool> OnCheatModeChange;

    private bool IsCheatMode
    {
        get { return IsCheatMode; }
        set {  _IsCheatMode = value;  OnCheatModeChange.Invoke(value); }
    }    

    public void ToggleCheatMode(){
        IsCheatMode = !IsCheatMode;
    }


    //--------- Scene game states ------------
    public enum GameStateType  
    {
        Bootstrap,
        PlayerIdle,
        PlayerThrowing,
        FinishGame,
    }

    // Not all events are needed or used, but it is better for the architecture to state them
    public enum GameStateEventType  
    {
        BootstrapStart,
        BootstrapEnd,
        PlayerThrowingStart,
        PlayerThrowingEnd,
        FinishGameStart,
    }


    [System.NonSerialized]
    public UnityEvent<GameStateEventType> OnChangeGameState;

    private GameStateType _GameState = GameStateType.Bootstrap;

    public GameStateType GameState
    {
        get { return _GameState; }
        private set {
            _GameState = value;
        }
    }


    private GameObject[] GameTargets;
    private GameObject[] GameBalls;

    private void Start(){
        BootstrapGame(true);
    }

    private void OnEnable (){
        if (OnChangeGameState == null)
            OnChangeGameState = new UnityEvent<GameStateEventType>();

        if (OnScoreChange == null)
            OnScoreChange = new UnityEvent<int>();

        if (OnCheatModeChange == null)
            OnCheatModeChange = new UnityEvent<bool>();
    }
    
    public async void BootstrapGame( bool hardBootstrap = false){
        //Setting state-machine state and calling events
        OnChangeGameState.Invoke(GameStateEventType.BootstrapStart);
        GameState = GameStateType.Bootstrap;

        //Initializing reference to game object on hard bootstrap
        if(hardBootstrap){
            // Getting a reference for all related game object
            //TODO: ------- The probable error is in finding the objects properly --------
            GameTargets = GameObject.FindGameObjectsWithTag("GameTarget");
            GameBalls = GameObject.FindGameObjectsWithTag("GameBall");
        }

        // Reseting common game variables
        TotalScore = 0;

        List<Task> tasks = new List<Task>();
        foreach(var target in GameTargets){
            TargetCollisionManager temp = target.GetComponent<TargetCollisionManager>();
            tasks.Add( new Task( () => { 
                temp.SetDebug("Random Text");
                temp.InitTarget(); 
                } ) );
        }

        //Check that all balls and all targets are correctly initialized
        await Task.WhenAll(tasks);


        OnChangeGameState.Invoke(GameStateEventType.BootstrapEnd);
        GameState = GameStateType.PlayerIdle;
    }

    // <summary>
    // Activated when the user grabs a ball
    // </summary>
    public void Game_StartThrowing(){
        OnChangeGameState.Invoke(GameStateEventType.PlayerThrowingStart);
        GameState = GameStateType.PlayerThrowing;

        // TODO: Calculate cheat mode center of mass and activate cheat gravity

        // TODO: Check for all target items to stop moving -> Subscribe to all target items velocities

    }

    // <summary>
    // Activated when all target items stopped moving after a ball was thrown
    // </summary>
    private void Game_ThrowingEnd(){
        //TODO : If all cans are inactive, the user has finished the game
    }





}
