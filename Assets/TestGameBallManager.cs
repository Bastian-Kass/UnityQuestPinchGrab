using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class TestGameBallManager : MonoBehaviour
{

    [SerializeField]
    public GameBallManager gbManager;

    private void FixedUpdate(){
        

        var keybooard = Keyboard.current;
        if (keybooard == null)
            return; // No gamepad connected.

        if (keybooard.spaceKey.wasPressedThisFrame)
        {   
            Debug.Log("Space key");
            gbManager._rigidbody.velocity = new Vector3 (1f, 20f, -2f);
        }

    }
}
