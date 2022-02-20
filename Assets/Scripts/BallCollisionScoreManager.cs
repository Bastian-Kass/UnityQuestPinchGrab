using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCollisionScoreManager : MonoBehaviour
{

    [SerializeField]
    private ThrowGameSO throwGameManager;
    // AudioSource audioSource;

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("GameBall")){
            int magnitud = (int)collision.relativeVelocity.sqrMagnitude;
            throwGameManager.addToScore(magnitud);
        }
    }
}
