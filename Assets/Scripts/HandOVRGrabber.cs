using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Modified OVRGrabber to work with hand gesture pinch
/// Author Sebastian Casillas
/// </summary>
public class HandOVRGrabber : MonoBehaviour
{

    // Should be OVRInput.Controller.LTouch or OVRInput.Controller.RTouch.
    [SerializeField]
    public OVRInput.Controller m_controller;

    [SerializeField]
    private bool m_parentHeldObject = false;


    // If true, this script will move the hand to the transform specified by m_parentTransform, using MovePosition in
    // Update. This allows correct physics behavior, at the cost of some latency. In this usage scenario, you should 
    // NOT parent the hand to the hand anchor.
    // (If m_moveHandPosition is false, this script will NOT update the game object's position. The hand gameObject 
    // can simply be attached to the hand anchor, which updates position in LateUpdate, gaining us a few ms of reduced 
    // latency.)
    [SerializeField]
    private bool m_moveHandPosition = false;

    // You can set this explicitly in the inspector if you're using m_moveHandPosition.
    // Otherwise, you should typically leave this null and simply parent the hand to the hand anchor
    // in your scene, using Unity's inspector.
    [SerializeField]
    private Transform m_parentTransform;

    // Child/attached transforms of the grabber, indicating where to snap held objects to (if you snap them).
    // Also used for ranking grab targets in case of multiple candidates.
    [SerializeField]
    private Transform m_gripTransform = null;
    // Child/attached Colliders to detect candidate grabbable objects.
    [SerializeField]
    private Collider[] m_grabVolumes = null;

    private Renderer m_renderer;


    private bool m_grabVolumeEnabled = true;

    private Vector3 m_lastPos;
    private HandOVRGrabbable m_grabbedObj = null;
    private Rigidbody m_grabbedObj_Rigidbody = null;

    private Dictionary<HandOVRGrabbable, int> m_grabCandidates = new Dictionary<HandOVRGrabbable, int>();
    private bool m_operatingWithoutOVRCameraRig = true;

    bool m_prev_grab = false;
    public OVRInput.Button trigger;

    private CircularFloatArray circularVelocityArray;



    public HandOVRGrabbable grabbedObject
    {
        get { return m_grabbedObj; }
    }


    public void Start()
    {
        //Getting hand
        OVRSkeleton skeleton = gameObject.GetComponentInParent<OVRSkeleton>();

        // Locating the hand index and moving the trigger object to the tip
        foreach (OVRBone bone in skeleton.Bones)
            if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
            {
                gameObject.transform.SetParent(bone.Transform);
                gameObject.transform.Translate(0.18f, -0.01f, 0.015f);
            }
                
            
        // Referencing the renderer for debuging purposes
        m_renderer = gameObject.GetComponent<Renderer>();

        //Got to have first values for the release not to crash
        m_lastPos = transform.position;

        circularVelocityArray = new CircularFloatArray(10);
    }

    public void Awake()
    {
        if (!m_moveHandPosition)
        {
            // If we are being used with an OVRCameraRig, let it drive input updates, which may come from Update or FixedUpdate.
            OVRCameraRig rig = transform.GetComponentInParent<OVRCameraRig>();

            if (rig != null)
            {
                rig.UpdatedAnchors += (r) => { OnUpdatedAnchors(); };
                m_operatingWithoutOVRCameraRig = false;
            }
        }
    }

    public void Update()
    {
        // Running in update anchors (movement and pinch check) only if there is no camera rig
        if (m_operatingWithoutOVRCameraRig)
            OnUpdatedAnchors();
    }

    void OnUpdatedAnchors()
    {

        Vector3 velocity = (transform.position - m_lastPos) / Time.deltaTime;
        circularVelocityArray.addItem(velocity);

        MoveGrabbedObject(transform.position);
        CheckForGrabOrRelease();

        //Storing values for next iteration -> Useful when calculating release velocity
        m_lastPos = transform.position;

    }

    void CheckForGrabOrRelease()
    {

        bool grab = OVRInput.Get(trigger);

        // Change in grab position
        // TODO: Could be using a pinch force as threshold
        if(grab != m_prev_grab)
        {
            // Grab init and no object being grabbed
            if (grab)
            {
                if (m_grabbedObj == null)
                    GrabBegin();
            }
            // Grab ending and an object being grabbed
            else
            {
                if (m_grabbedObj != null)
                    GrabEnd();
  
            }
        }

        m_prev_grab = grab;

    }

    void OnTriggerEnter(Collider otherCollider)
    {
        // Get the grab trigger [checking for null]
        HandOVRGrabbable grabbable = otherCollider.GetComponent<HandOVRGrabbable>() ?? otherCollider.GetComponentInParent<HandOVRGrabbable>();

        if (grabbable == null) 
            return;

        //Same as parent class, I just added this method so set up color indicator
        grabbable.GetComponent<Renderer>().material.color = Color.green;

        // Add the grabbable
        int refCount = 0;
        m_grabCandidates.TryGetValue(grabbable, out refCount);
        m_grabCandidates[grabbable] = refCount + 1;
    }

    void OnTriggerExit(Collider otherCollider)
    {
        HandOVRGrabbable grabbable = otherCollider.GetComponent<HandOVRGrabbable>() ?? otherCollider.GetComponentInParent<HandOVRGrabbable>();

        if (grabbable == null) 
            return;


        //Same as parent class, I just added this method so set up color indicator
        grabbable.GetComponent<Renderer>().material.color = Color.blue;

        // Remove the grabbable
        int refCount = 0;
        bool found = m_grabCandidates.TryGetValue(grabbable, out refCount);

        if (!found)
            return;
        

        if (refCount > 1)
            m_grabCandidates[grabbable] = refCount - 1;
        else
            m_grabCandidates.Remove(grabbable);
        
    }

    private void GrabBegin()
    {
        float closestMagSq = float.MaxValue; // Comparing with max float value
        HandOVRGrabbable closestGrabbable = null;

        // Iterate grab candidates and find the closest grabbable candidate
        foreach (HandOVRGrabbable grabbable in m_grabCandidates.Keys)
        {

            // Ignoring already grabbed items
            if (grabbable.isGrabbed)
                continue;
            
            // Loocing for grabbable points
            // TODO: Simplify, for balls there is only one point
            for (int j = 0; j < grabbable.grabPoints.Length; ++j)
            {
                Collider grabbableCollider = grabbable.grabPoints[j];
                // Store the closest grabbable
                Vector3 closestPointOnBounds = grabbableCollider.ClosestPointOnBounds(m_gripTransform.position);
                float grabbableMagSq = (m_gripTransform.position - closestPointOnBounds).sqrMagnitude;
                if (grabbableMagSq < closestMagSq)
                {
                    closestMagSq = grabbableMagSq;
                    closestGrabbable = grabbable;
                }
            }


        }

        // Disable grab volumes to prevent overlaps
        GrabVolumeEnable(false);


        if (closestGrabbable != null)
        {

            m_grabbedObj = closestGrabbable;
            m_grabbedObj_Rigidbody = m_grabbedObj.GetComponent<Rigidbody>();

            m_grabbedObj.GrabBegin(this, m_grabbedObj.GetComponent<Collider>());

            MoveGrabbedObject(transform.position, true);

        }

    }

    private void GrabEnd()
    {

        if (m_grabbedObj != null)
        {

            Vector3 velocity = (transform.position - m_lastPos) / Time.deltaTime;


            circularVelocityArray.addItem(velocity);


            Vector3 linearVelocity = circularVelocityArray.getAverage();

            GrabbableRelease(linearVelocity);
        }

        // Re-enable grab volumes to allow overlap events
        GrabVolumeEnable(true);
    }


    public void GrabbableRelease(Vector3 linearVelocity)
    {


        m_grabbedObj.GrabEnd(linearVelocity);

        if (m_parentHeldObject) 
            m_grabbedObj.transform.parent = null;

        m_grabbedObj_Rigidbody = null;
        m_grabbedObj = null;
        
    }

    public void ForceRelease(HandOVRGrabbable grabbable)
    {
        bool canRelease = (
            (m_grabbedObj != null) &&
            (m_grabbedObj == grabbable)
        );
        if (canRelease)
        {
            GrabEnd();
        }
    }

    public void MoveGrabbedObject(Vector3 pos, bool forceTeleport = false)
    {
        // There needs to be a grabbed body
        if (m_grabbedObj == null || m_grabbedObj_Rigidbody == null)
            return;

        Vector3 grabbablePosition = pos;

        // Saving last velocity instead of last position

        if (forceTeleport)
            m_grabbedObj_Rigidbody.transform.position = grabbablePosition;
        else
            m_grabbedObj_Rigidbody.MovePosition(grabbablePosition);
        

    }

    private void GrabVolumeEnable(bool enabled)
    {
        if (m_grabVolumeEnabled == enabled)
            return;
        

        m_grabVolumeEnabled = enabled;

        for (int i = 0; i < m_grabVolumes.Length; ++i)
        {
            Collider grabVolume = m_grabVolumes[i];
            grabVolume.enabled = m_grabVolumeEnabled;
        }

        if (!m_grabVolumeEnabled)
            m_grabCandidates.Clear();
        
    }

}
public class  CircularFloatArray
{

    private Vector3[] items;
    private int index = 0;

    private bool circularRoundStateFlag = false;

    private int size;
    public CircularFloatArray(int size)
    {
        this.size = size;
        // The array cannot be null
        if (size<=0) 
            throw new ArgumentException("There needs to be a size of minimum 1");

        //Initializing the circular array
        items = new Vector3[size];
    }

    public void addItem(Vector3 v){

        if(index == size){
            circularRoundStateFlag = true;
            index = 0;
        }
        
        items[index] = v;

    }

    public Vector3 getAverage(){
        //We need average speed and average trayectory
        
        Vector3 average = Vector3.zero;

        // TODO: weight each vector by the distance to the one before it to get a nice curve

        for (int i = 0; i < items.Length; i++)
            average += items[i];

        return average;

    }



}