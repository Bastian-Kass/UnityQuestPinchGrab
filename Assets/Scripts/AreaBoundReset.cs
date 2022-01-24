using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaBoundReset : MonoBehaviour
{

    public GameObject respawnBox;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider otherCollider)
    {
      if(otherCollider.tag == "BoundryBox"){
          gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
          gameObject.transform.position = respawnBox.transform.position;
      }


        
    }
}
