using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CenterOfMassHelper : MonoBehaviour
{

    public Vector3 CenterOfMass2;
    public bool Awake;
    protected Rigidbody _rigidbody;

    public float massGizmoRadio = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        _rigidbody.centerOfMass = CenterOfMass2;
        _rigidbody.WakeUp();
        Awake = !_rigidbody.IsSleeping();
    }

    private void OnDrawGizmos(){
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + transform.rotation * CenterOfMass2, massGizmoRadio);
    }
}
