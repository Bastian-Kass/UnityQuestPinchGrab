# UnityQuestPinchGrab
### Test project in aims to experience the Unity development environment paired with OculusSDK 3.7 interaction plugin and assets
The game consist in a simple "Throw the balls to make the soda can tower collapse". 
Additionally, there is a "Cheat"/"Assist" mode to correct the path of the ball.

# Installation specifications
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

## Configurations:
* To compile the project the 'build settings' might need to be changed to Android upon first access


# Game Logic


## Interactions in VR with the Meta Quest/Oculus Quest 2

To make things work, this project is heavily based on the **OculusInteractionRig** provided in the *OculusIntegration 3.7* asset, and within the "Complex Hand Interactions" Scene; This implementation was then further simplified to only cover the "grab" and "poke" gestures for both controller and hand-detection interactions. These are set to work with 3 balls [Prefab game object] and a floating menu menu by assigning the proper scripts [Interactions: Grab Interactable, Transformable, One Hand Free Transform, Snap Point, Poke Interactable].

### User experience within the VR environment
By using the **Oculus Hand and Controller Interaction assets** and customizing them through their parameters, it is then possible to achieve a throw interaction that feels natural the the user. As a bit of context, the Oculus interactable assets work by saving a set amount of positions and velocities that precede the event/gesture that signals the end of the grab interaction; the algorithm then calculates a velocity by purging and weighing the different saved values. For the time being, the project uses a copy of the "ThrowVelocityCalculator" and applies the following custom parameters obtained holistically:

![Screenshot 2022-03-25 134535](https://user-images.githubusercontent.com/6613145/160115128-b792f1ef-c268-4e69-8f24-79f3f02d3f7d.jpg)

## Integrating the game objects with the Interaction Rig
To work together with the Oculus Interaction Rig, the project implements the throwing objects(with their respective GrabInteractable, SnapPoint, and Transform related scripts). Each ball also implements a custom script to manage audio, debugging visuals, position to reset to whenever a game starts, and communication with the general Game Manager script to set the cumulative score.

## Target behavior
Likewise, each target contain the TargetCollisionManager script to manage the audio on collision, general object looks (changing from red to grey when target is not longer active), and to communicate the cumulative score with the general GameManager script.

## Global Game Management
Finally, the project includes an object with the general GameManager script. This script handles the cumulative score, and communicates with other game object through events and references in order to handle the state of the game given a certain logic or interaction:

## Cheat/Assist Mode
The most challenging part of this development was making the game interactions feel natural, and this includes a polished way to implement the cheat/assist mode. Consequentially, and involving various iterations of "trial and error" research, this mode was designed as follows:

1) To assist the player, the game can be set so the thrown balls are attracted to the targets.
2) To do so, whenever the player makes a throw, we create an object between the player and the targets(center of mass of the targets) that will serve as a pivotal point to attract the ball.
3) It is essential that the ball's trajectory is only affected to its relative left or right. 
   * If the attraction has an effect on the axis of the ball's trajectory, the ball gains a "wonky" behavior whenever it exponential accelerates towards the targets; even worse when the ball even stops its current trajectory and returns towards the targets given a strong enough attraction.
   * If the attraction has an effect on the gravitational axis, the ball gains an usual behavior whenever it is thrown over the targets thus accelerates downwards unnaturally, or when it reaches a lower location than the targets and starts defying gravity in response to this.

To achieve the envisioned outcome, the implementation calculates the orthonormal vector to the **ball's direction** and the **upwards direction vector** in order to keep track of the right side of the initial trajectory; the ball is then iteratively attracted to the previous calculated object[as mentioned in step 2]. The result in a top-down view and some additional visuals looks like can be seen in the following video:

https://user-images.githubusercontent.com/6613145/160112832-198b8467-b0db-4517-b140-62552d10564f.mp4



### Area bounds and colliders for game logic
The game makes use of three main areas to assist in determining the state of the game.

* GameBounds [Collider/Trigger]: Whenever a ball leaves this area a time counter, and after certain time the ball is marked as inactive.
  * Whenever all balls are set to inactive, now matter the amount of ball sin the scene, the GameManager script knows that the game has ended.
  * Additionally, this stops occurrences such as a falling target object hitting a ball in its way down and counting towards the final score.

* ThrowingBounds [Collider/Trigger]: Whenever a ball leaves the area that surrounds the player it is set as a "Thrown" ball. This comes extremely useful to easily determine when a ball has been thrown without overbearing the system by detecting the "Thrown" event.

  * Whenever the "Assist"/"Cheat" mode is active, a ball leaving the player area signals the system to calculate and instantiate the object which will generate this artificial attraction force.
  * Useful to add a "Swooshing" sound effect to the ball whenever it has been thrown.

* ActiveTarget area[Collider/Trigger]: Whenever a target leaves this area, any hit to it will not count towards the final score. This comes handy to differentiate hits such as:
  * A hit from a thrown ball to a target inside the are should count towards the score.
  * A hit from a ball that has bounced after hitting another target should count towards the score.
  * A hit from a ball to a target that is laying on the ground(outside the area) should not count towards the score.

### Game score
The game score is calculated by addition in two distinct events:
* The square magnitude of the collision relative velocity between an active ball and an active target [times a multiplier]
  * Multiple hits can happen each throw whenever the ball bounces between targets.
* The square magnitude of the distance between the initial target position and the final position [times a multiplier]
  * Only for targets that left the active-target zone
* The number of unused balls [times 5000]


# Important notes
* Oculus Quest 2 is not perfect; controller detection and hand detection success vary greatly from the context of the player. For example, how much light is in the room whenever the player is using hand-detection greatly affects its effectiveness.
* The first instances of this project involved creating my own implementation the grabbing functionality by using the auto-detection of the "pinch" gesture in the old "Oculus Integration suite 3.5". However, since its update to 3.7, Meta(Previously known as Oculus) provides a much more robust implementation that encompasses complex grab with both hardware or hand-detection only controls.

# Work for future iterations

* The hand visualization of the Oculus Quest integration are provided as collidable, and sometimes hit the balls when their location is not determined correctly by the headset. They should be set to not collide with objects themselves.
