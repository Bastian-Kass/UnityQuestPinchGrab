using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatBlackHole : MonoBehaviour
{

    private bool IsCheatModeActive = false;

    [SerializeField]
    private GameManagerScript throwGameManager;

    void OnEnable()
    {
        throwGameManager.OnCheatModeChange.AddListener(SetCheatMode);
    }

    void OnDisable()
    {
        throwGameManager.OnCheatModeChange.RemoveListener(SetCheatMode);
    }

    private void SetCheatMode(bool b){
        IsCheatModeActive = b;
    }

    private void Update(){

    }


}
