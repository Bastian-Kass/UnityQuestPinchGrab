using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ScoreBoardManager : MonoBehaviour
{

    public Text text;

    [SerializeField]
    public GameManagerScript throwGameSO;

    private void OnEnable(){
        throwGameSO.OnScoreChange.AddListener(ChangeScoreInBoard);
    }

    private void OnDisable(){
        throwGameSO.OnScoreChange.RemoveListener(ChangeScoreInBoard);
    }

    private void ChangeScoreInBoard(int score){
        text.text = score.ToString();
    }
    
    private void setBoardText(string s){
        text.text = s;
    }

}
