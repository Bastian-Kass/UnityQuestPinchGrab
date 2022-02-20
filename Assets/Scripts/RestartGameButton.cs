using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartGameButton : MonoBehaviour
{

    [SerializeField]
    private ThrowGameSO throwGameManager;
    public void RestartGame(){
        throwGameManager.RestartGame();
    }
}
