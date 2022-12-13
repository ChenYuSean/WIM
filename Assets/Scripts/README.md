# Tracing
All the main function starting at `DroneCasting.Cs` and `Wim.cs` under "[CameraRig]" <br />
`GameManager.cs` is a static singleton function storing pointer to other manager scripts <br />
Collision detection is independent from other script, running by Unity with OnCollision Functions every frame <br />
see `ConeCollision.cs`, `DetectTouch.cs`, `ArrowTrigger.cs`, `RoiGrab.cs`, `TriggerSensor.cs`

# GameObject
Name: Function<br />
 
- Manager : GameManager and other manager container 
- CameraRig : Playing Area and Main Script
- CameraRig/Controller/Arrow: Collider Dectection of Controller
- CameraRig/Teleport: Teleporting 
- CameraRig/WimPos: 6DOF of 2 wim 
- CameraRig/CastingUtil: utiliy game object useding drone cast 
- DroneScanner: Drone 
- World: Wim generation region
- World/WimBoundary : Boundary for wim after generate 
- World/ROI: Roi of Wim
- World/Avatar: Avatar on Wim 


# Reference Assign
Variable <- GameObject<br />

Manager/GameManger.cs:
- SFX/0 <- Grab
- SFX/1 <- Select
- CameraRig <- [CameraRig]
- AudioMgr <- CameraPos(AudioManager)

CameraRig/Wim.cs:
- Global Wim Default Pos <- [CameraRig]/WimPos/GlobalWimDefaultPos
- Local Wim Default Pos <- [CameraRig]/WimPos/LocalWimDefaultPos

CameraRig/DroneCasting.cs:
- IM <- Manager(InputManager)
- DroneScanner <- DroneScanner
- CastingUtil <- [CameraRig]/CasingUtil
- Right Hand Arrwow <- Controller(right)/Arrow
- Left Hand Arrwow <- Controller(left)/Arrow
- RotationAxis <- [CameraRig]/CasingUtil/RotationAxis

# Tag & Layer (Assign Before Run)
Layer:
- CollisionDetect: Used for unity physic collision (might not used)
- Unchangeable: Ingore during Drone scanning and Wim
- SelectableBackground: Can be shown on Wim and Drone
- Background: can only be shown on wim