using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartButtonManager : MonoBehaviour
{

    [SerializeField]
    private GameManagerScript throwGameManager;
    public void RestartGame(){
        throwGameManager.BootstrapGame();
    }
}
