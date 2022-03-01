using UnityEngine;
using TMPro;

public class ScoreBoardManager : MonoBehaviour
{
    public TextMeshPro text;

    [SerializeField]
    public GameManagerScript gameManager;

    private void OnEnable(){
        gameManager.OnScoreChange.AddListener(ChangeScoreInBoard);
    }

    private void OnDisable(){
        gameManager.OnScoreChange.RemoveListener(ChangeScoreInBoard);
    }

    private void ChangeScoreInBoard(int score){
        text.text = score.ToString();
    }
    
    private void setBoardText(string s){
        text.text = s;
    }

}
