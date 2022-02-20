using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateCheatMode : MonoBehaviour
{

    [SerializeField]
    private ThrowGameSO throwGameManager;
    public void StartCheatMode(){
        throwGameManager.SetCheatMode(true);
    }

    public void EndCheatMode(){
        throwGameManager.SetCheatMode(false);
    }
}
