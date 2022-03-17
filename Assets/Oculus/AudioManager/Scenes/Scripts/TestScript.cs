using UnityEngine;


public class TestScript : MonoBehaviour {


	// Use this for initialization
	void Awake () {

#if UNITY_EDITOR
    Debug.Log("Only called in the editor.");
#endif

	}
	


}
