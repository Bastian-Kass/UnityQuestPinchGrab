# UnityQuestPinchGrab
### Test project in aims to experience Unity 3D development paired with OculusSDK 3.7
The game consist in a simple "Throw the balls and collapse the can tower". Additionally, there is a "cheatmode" which involves math in the 3D space and understanding of the unity engine to correct the path of the ball when thrown thus assist the player on the task.

# Instalation specifications
Unity version: 2020.3.30f
## Used Packages
* OculusSDK XR Plugin 3.0.0 (Preview 2)
* OpenXR Plugin 1.3.1
* XR Interaction Toolkit 2.0.1
* XR Plugin Management 4.2.1
* TextMeshPro 3.0.6
## Used Additional Assets
* Oculus Integration 37.0
* Quick Outline 1.0.5

## Note:
*To compile the project the 'build settings' might need to be changed to Android upon first access*


## Abstract
The first instances of this project involved creating my own implementation the grabbing functionality by using the autodetection of the "pinch" gesture in the old "Oculus Integration suite 3.5". However, since its update to 3.7, Meta(Previously known as Oculus) provides a much more robust implementation that encompases complex grab with both hardware or hand-detection only controls.

To make things work, this project is heavily based on the OculusInteractionRig provided in the OculusIntegration 3.7 within the "Complex Hand Interactions" Scene. It was further simplified to only encompass the "grab" and "poke" gestures for both controller and hand-detection interactions. Both are then set to work with 3 distinct balls and a floating button menu by assigning the proper scripts[Interactors: Grab Interactable, Transformable, One Hand Free Transform, Snap Point, Poke Interactable]

# Game Logic
As previously mentioned, the game consists in throwing three balls and making a soda-can tower collapse.

...In progress

![alt text](http://url/to/img.png)

### Area bounds and colliders for game logic
The game makes use of two main areas; the first one to determine where the player should be, and the second one to determine where the gamplay should reside. These and some other interactions between objects are the following:

* GameBounds: Set as a collider trigger. Whenever a ball leaves this area, a counter is set and closely followed by the action of making the ball inactive. An inactive ball means that is has been thrown and can no longer be used for any in-game logic.
  
    * This makes a difference for very specific instances; for example, imagine that a sub-sequent ball makes a can fly towards a ball that has already been thrown; this should not count to the final score.

* ThrowingBounds: Whenever a ball leaves the area that surrounds the player it is set as a "Thrown" ball. This comes extremely usefull to easily determine when a ball has been thrown without overbearing the system by detecting the "Thrown" event.

  * Useful to add a "Swooshing" sound effect to the ball whenever it has been thrown.

* Each throwable object(Ball) has two main interactions. Whenever it hits a gametarget(soda-can) it calculates a score and adds it to the tally on the scene-bound GameManager. Additioonally, with each collision over a specific threshold, it has a nice collison sound for game immersion.

* If a ball fell to the ground, either by mistake or a terrible throw, it will automatically reset its position to the provided container to the right for easy access.

# Important notes
Oculus Quest 2 is not perfect; controller detection and hand detection success vary gratly from the context of the player. For example, how much light is in the room whenever the player is using hand-detection greatly affects its effectiveness.

# Work for future iterations

* The hand visualization of the Oculus Quest integration are provided as colidable, and sometimes hit the balls when their location is not determined correctly by the headset. They should be set to not collide with objects themselves.
* 
