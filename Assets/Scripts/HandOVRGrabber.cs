using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Allows grabbing things with a specific trigger
/// </summary>
public class HandOVRGrabber : MonoBehaviour
{
    public bool debug = false;
    public Text debug_txt;

    // Should be OVRInput.Controller.LTouch or OVRInput.Controller.RTouch.
    [SerializeField]
    public OVRInput.Controller m_controller;

    [SerializeField]
    private bool m_parentHeldObject = false;


    // If true, this script will move the hand to the transform specified by m_parentTransform, using MovePosition in
    // Update. This allows correct physics behavior, at the cost of some latency. In this usage scenario, you
    // should NOT parent the hand to the hand anchor.
    // (If m_moveHandPosition is false, this script will NOT update the game object's position.
    // The hand gameObject can simply be attached to the hand anchor, which updates position in LateUpdate,
    // gaining us a few ms of reduced latency.)
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
    private Quaternion m_lastRot;

    private Quaternion m_anchorOffsetRotation;
    private Vector3 m_anchorOffsetPosition;

    private HandOVRGrabbable m_grabbedObj = null;
    private Vector3 m_grabbedObjectPosOff;
    private Quaternion m_grabbedObjectRotOff;

    private Dictionary<HandOVRGrabbable, int> m_grabCandidates = new Dictionary<HandOVRGrabbable, int>();
    private bool m_operatingWithoutOVRCameraRig = true;

    bool m_prev_grab = false;
    public OVRInput.Button trigger;

    private Queue<Vector3> location_history = new Queue<Vector3>();

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

        m_lastPos = transform.position;
        m_lastRot = transform.rotation;

        if (m_parentTransform == null)
            m_parentTransform = gameObject.transform;
        
    }

    public void Awake()
    {
        m_anchorOffsetPosition = transform.localPosition;
        m_anchorOffsetRotation = transform.localRotation;

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
        MoveGrabbedObject(transform.position, transform.rotation);
        MyCheckForGrabOrRelease();
    }

    void MyCheckForGrabOrRelease()
    {

        bool grab = OVRInput.Get(trigger);

        // Change in grab position
        // TODO: Could be using a pinch force as threshold
        if(grab != m_prev_grab)
        {
            // Grab init and no object being grabbed
            if (grab)
            {
                if(debug)
                    m_renderer.material.color = Color.cyan;

                if (m_grabbedObj == null)
                    GrabBegin();
            }
            // Grab ending and an object being grabbed
            else
            {
                if (debug)
                    m_renderer.material.color = Color.white;

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
        if (debug)
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
        if (debug)
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
        float closestMagSq = float.MaxValue;
        HandOVRGrabbable closestGrabbable = null;

        // Iterate grab candidates and find the closest grabbable candidate
        foreach (HandOVRGrabbable grabbable in m_grabCandidates.Keys)
        {


            bool canGrab = !(grabbable.isGrabbed && !grabbable.allowOffhandGrab);
            if (!canGrab)
            {
                continue;
            }



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

            DetermineOnGrabOffset();

            m_grabbedObj.GrabBegin(this, m_grabbedObj.GetComponent<Collider>());

            m_lastPos = transform.position;
            m_lastRot = transform.rotation;

            MoveGrabbedObject(m_lastPos, m_lastRot, true);

        }

    }

    private void GrabEnd()
    {

        if (m_grabbedObj != null)
        {
            OVRPose localPose = new OVRPose { 
                        position = OVRInput.GetLocalControllerPosition(m_controller), 
                        orientation = OVRInput.GetLocalControllerRotation(m_controller) 
                        };

            OVRPose offsetPose = new OVRPose { 
                        position = m_anchorOffsetPosition, 
                        orientation = m_anchorOffsetRotation 
                        };

            localPose = localPose * offsetPose;

            OVRPose trackingSpace = transform.ToOVRPose() * localPose.Inverse();
            Vector3 linearVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerVelocity(m_controller);
            Vector3 angularVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerAngularVelocity(m_controller);

            GrabbableRelease(linearVelocity, angularVelocity);
        }

        // Re-enable grab volumes to allow overlap events
        GrabVolumeEnable(true);
    }


    public void GrabbableRelease(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        
        m_grabbedObj.GrabEnd(linearVelocity, angularVelocity);

        if (m_parentHeldObject) 
            m_grabbedObj.transform.parent = null;

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

    public void MoveGrabbedObject(Vector3 pos, Quaternion rot, bool forceTeleport = false)
    {
        // There needs to be a grabbed body
        if (m_grabbedObj == null)
            return;



        //TODO: For now ignoring position and rotation
        Vector3 grabbablePosition = pos + rot * m_grabbedObjectPosOff;
        Quaternion grabbableRotation = rot * m_grabbedObjectRotOff;

        Rigidbody grabbedRigidbody = m_grabbedObj.GetComponent<Rigidbody>();

        if (forceTeleport)
        {
            grabbedRigidbody.transform.position = m_grabbedObjectPosOff;
            grabbedRigidbody.transform.rotation = m_grabbedObjectRotOff;
        }
        else
        {
            grabbedRigidbody.MovePosition(m_grabbedObjectPosOff);
            grabbedRigidbody.MoveRotation(m_grabbedObjectRotOff);
        }

    }

    private void GrabVolumeEnable(bool enabled)
    {
        if (m_grabVolumeEnabled == enabled)
        {
            return;
        }

        m_grabVolumeEnabled = enabled;
        for (int i = 0; i < m_grabVolumes.Length; ++i)
        {
            Collider grabVolume = m_grabVolumes[i];
            grabVolume.enabled = m_grabVolumeEnabled;
        }

        if (!m_grabVolumeEnabled)
        {
            m_grabCandidates.Clear();
        }
    }

    private void OffhandGrabbed(HandOVRGrabbable grabbable)
    {
        if (m_grabbedObj == grabbable)
        {
            GrabbableRelease(Vector3.zero, Vector3.zero);
        }
    }

    private void DetermineOnGrabOffset(){
        if(m_grabbedObj.snapPosition)
            {
                m_grabbedObjectPosOff = m_gripTransform.localPosition;
                if(m_grabbedObj.snapOffset)
                {
                    Vector3 snapOffset = m_grabbedObj.snapOffset.position;
                    if (m_controller == OVRInput.Controller.LTouch) snapOffset.x = -snapOffset.x;
                    m_grabbedObjectPosOff += snapOffset;
                }
            }
            else
            {
                Vector3 relPos = m_grabbedObj.transform.position - transform.position;
                relPos = Quaternion.Inverse(transform.rotation) * relPos;
                m_grabbedObjectPosOff = relPos;
            }

            if (m_grabbedObj.snapOrientation)
            {
                m_grabbedObjectRotOff = m_gripTransform.localRotation;
                if(m_grabbedObj.snapOffset)
                {
                    m_grabbedObjectRotOff = m_grabbedObj.snapOffset.rotation * m_grabbedObjectRotOff;
                }
            }
            else
            {
                Quaternion relOri = Quaternion.Inverse(transform.rotation) * m_grabbedObj.transform.rotation;
                m_grabbedObjectRotOff = relOri;
            }
    }

}
