# UnityQuestPinchGrab
### Test project to implement grabbing things with the oculusSDK and the hand-based pinching gesture

# Instalation warning
*To compile the project the 'build settings' might need to be changed to Android upon first access*


## Abstract
Since the OculusSDK prefabs for OVRGrabber and OVRGrabbable and not prepared to work with hand gestures(only with quest controllers and triggers), I worked in this simple project to modify the given classes and implement them myself.
The main scene under Assets/Scenes/MainScene contains the following:
 * A table and a bowl for the object that can be grabbed and thrown
 * A panel with text objects for debugging purposes
 * The objects that will we will be able to grab and throw
 ** These objects must have a rigid body and the custom script HandOVRGrabbable
 * Default OVRCameraRig with the hand prefab attached to the hand anchors
 ** A hand that can grab needs to have a 3D object attached to detect the collision with the grabbable object, and the script HandOVRGrabber attached to it
 * Cubes used as bounding box objects to reset the objects that go beyond our reach

## HandOVRGrabbable
Basically the default script copied over; nothing changed here except the permissions to some variables; This was necessary since the original class was meant to be overridable but did not give proper access to all of its methods[those that were not made virtual].
It also makes a better implementation to override both classes(OVRGrabber and OVRGrabbable to work together nicely)

## Hand OVRGrabber
Same as HandOVRGrabbable, the class was copied over and not extended since not all the methods that required override were made virtual; additionally, some core components changed in the class so it made sense to create a new one instead of just overriding everything.

**Makes use of a 3D object and TriggerEnter/TriggerExit to detect collision against grabbable objects, and initialize the grab sequence when detecting certain input**

Major changes include:
* Public settable variable for the trigger to be detected for the grabbing motion to begin
** OVRInput.Button.One is activated [through OVRInput.Get] when the right hand pinch gesture is detected
** Other gestures at: https://developer.oculus.com/documentation/unity/unity-ovrinput/
* Initialize useful variables to make code more efficient instead of getting them each iteration
** m_renderer : Reference to the 3D object used as trigger to color it with debugging purposes
** m_grabbedObj : Reference to the grabbed object; multiple purposes
** m_grabbedObj_Rigidbody: Reference to the grabbed object rigidbody to move it when grabbed
* Changes to the Start method to move the 3D trigger object to the tip of the index finger
* Changes in the Grab/Release checking method [MyCheckForGrabOrRelease] as to detect generic input from trigger instead of controller buttons
** It also adds materiall color change to the 3D trigger object for debugging purposes
* Adding color change to OnTriggerEnter/OnTriggerExit to the grabbable objects with debugging purposes
* For now; not determining the offset of rotation and translation when grabbing objects, the grabbed object goes to the pivot of the 3D trigger object.
** Some gains in efficiency
* Major changes to the calculation of velocity on GrabEnd; necesarry to produce a throwing motion on the grabbable object


## Area Bound Reset

Custom script that detects colission to items tagged as *BoundryBox* and resets the item to the location of the object tagged as 'RespawnBox'. Attached to the HandOVRGrabbable to reset the to the bowl when thwon out of hand's reach.

# Work for future iterations

* Due to the natural motion when throwing an object, basing the velocity on only the last two positions of the hand may result in velocities that are 'pointing down'. For better UX it would be interesting to store tha last 3-5 positions in a queue and weight them to determine a better trayectory for the throw.
* For better UX, the 3D object used as trigger should be transparent and glowing to let the grabbed object be seen upon grab
* Decide what happend on bad/poor hand recognition
