using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPositionOnRestart : MonoBehaviour
{
    // Start is called before the first frame update
    private Vector3 _initial_position;
    private Quaternion _initial_rotation;

    [SerializeField]
    private ThrowGameSO throwGameManager;
    // AudioSource audioSource;

    // Start is called before the first frame update
    void OnEnable()
    {
        throwGameManager.RestartGameEvent.AddListener(RestartPosition);
    }

    void OnDisable()
    {
        throwGameManager.RestartGameEvent.RemoveListener(RestartPosition);
    }

    private void Awake(){
        _initial_position = gameObject.transform.position;
        _initial_rotation = gameObject.transform.rotation;
    }


    private void RestartPosition(){
        gameObject.transform.position = _initial_position;
        gameObject.transform.rotation = _initial_rotation;
    }

}
