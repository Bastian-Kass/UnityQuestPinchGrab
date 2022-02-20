using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "GameData", menuName = "ScriptableObjects/ThrowGameSO", order = 1)]
public class ThrowGameSO : ScriptableObject
{

    [SerializeField]
    private int TotalScore;

    [SerializeField]
    private bool IsCheatMode = false;

    [System.NonSerialized]
    public UnityEvent<int> OnScoreChange;
    
    [System.NonSerialized]
    public UnityEvent RestartGameEvent;

    [System.NonSerialized]
    public UnityEvent<bool> CheatModeChange;

    private void setScore(int score){
        TotalScore = score;
        OnScoreChange.Invoke(TotalScore);
    }

    public void addToScore(int amount){
        setScore (TotalScore + amount);
    }

    public void RestartGame(){
        TotalScore = 0;
        RestartGameEvent.Invoke();
    }

    public void SetCheatMode(bool b){
        this.IsCheatMode = b;
        CheatModeChange.Invoke(b);
    }

    public int GetTotalScore(){
        return TotalScore;
    }

    private void OnEnable (){
        TotalScore = 0;

        if (OnScoreChange == null)
            OnScoreChange = new UnityEvent<int>();

        if (RestartGameEvent == null)
            RestartGameEvent = new UnityEvent();

        if (CheatModeChange == null)
            CheatModeChange = new UnityEvent<bool>();
    }





}
