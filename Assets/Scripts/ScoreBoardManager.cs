using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoardManager : MonoBehaviour
{

    public Text text;

    [SerializeField]
    private ThrowGameSO throwGameSO;

    private void OnEnable(){
        throwGameSO.OnScoreChange.AddListener(ChangeScoreInBoard);
        throwGameSO.RestartGameEvent.AddListener(ReactRestartGame);
        throwGameSO.CheatModeChange.AddListener(SetCheatModeText);
    }

    private void OnDisable(){
        throwGameSO.OnScoreChange.RemoveListener(ChangeScoreInBoard);
        throwGameSO.RestartGameEvent.RemoveListener(ReactRestartGame);
        throwGameSO.CheatModeChange.RemoveListener(SetCheatModeText);
    }


    private void ReactRestartGame(){
        setBoardText("New game started");
    }

    private void SetCheatModeText(bool b){
        setBoardText("Cheat Mode: " + b);
    }

    private void ChangeScoreInBoard(int score){
        text.text = score.ToString();
    }
    
    private void setBoardText(string s){
        text.text = s;
    }

}
