#region

using System;
using System.Collections.Generic;
using Enigmaware.Gravity;
using Enigmaware.World;
using KinematicCharacterController;
using System.Xml;
using UnityEngine;
using UnityEngine.Serialization;

#endregion

namespace Enigmaware.Motor
{
    public class KinematicNPCMotor : MonoBehaviour, ICharacterController
    {
        #region  Enums

        public enum MotorState
        {
            Default,
            Planet
        }
        
        public enum MovementType
        {
            // On the ground movement
            Ground,

            // In the air movement!
            Air,

            // Like sliding, but less cool.
            Crouching,
            
            // yeah
            RailGrinding,
            
            // For when the velocity of the character is set manually by an outside component (like a vehicle)
            Deferred
        }

        public enum BonusOrientationMethod
        {
            None,
            TowardsGravity,
            TowardsGroundSlopeAndGravity
        }

        #endregion
        #region Movement Types

        public void ForceMovementType(MovementType type)
        {
            lastMovementType = _currentMovementType;
            _currentMovementType = type;
        }

        #endregion
        #region Fields
        public Vector3 RootMotion;
        
        public KinematicCharacterMotor Motor;
        [SerializeField]
        public Transform MeshRoot;

        #region Ground Movement
        private float _cosineMaxSlopeAngle;

        [Header("Stable Movement")] [FormerlySerializedAs("MaxStableMoveSpeed")]
        public float PlanetGroundMovementSpeed = 10f;
        public float PlanetMovementSharpness = 15f;

        public float GroundMovementSpeed = 15f;
        public float GroundMovementAcceleration = 200f;
        public float MaxSlopeAngle = 50f;

        [Header("Crouching")]
        private bool _shouldBeCrouching;
        private bool _isCrouching;
        
        public float CrouchedCapsuleHeight = 1f;
        public float CrouchSpeed;
        public float CrouchDrag;
        public float CrouchAcceleration;
        public float CameraCrouchSpeed;
        
        public bool IsGrounded => Motor.GroundingStatus.IsStableOnGround;
        
        /// <summary>
        /// Used for the default state for when we're not on a planet
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="vel"></param>
        /// <param name="accel"></param>
        /// <param name="cap"></param>
        /// <returns></returns>
        private Vector3 GroundMove(Vector3 dir, Vector3 vel, float accel, float cap)
        {
            vel = ApplyFriction(vel, currentDrag);
            return Accelerate(vel, dir, accel, cap); //Accelerate(vel, dir, accel, cap);
        }

        /// <summary>
        /// Used for when we're on a planet
        /// </summary>
        /// <param name="currentVelocity"></param>
        private void GroundMove(ref Vector3 currentVelocity)
        {
            // Calculate target velocity
            Vector3 inputRight = Vector3.Cross(WishDirection, Up);
            Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized *
                                      WishDirection.magnitude;
            Vector3 targetMovementVelocity = reorientedInput * PlanetGroundMovementSpeed;

            // Smooth movement Velocity

            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
                1f - Mathf.Exp(-PlanetMovementSharpness * Time.deltaTime));
        }
        
        #endregion
        
        #region Sliding

        [Header("Sliding")] 
        public float SlideMinimumMagnitude;
        public float SlideBoost;
        public float SlideDrag;
        public float SlideAcceleration;
        public float SlideDelay;
        private float _slideTimer;

        #endregion
        
        #region Lurch

        [FormerlySerializedAs("lurch")]
        public AnimationCurve LurchCurve;
        
        private float _lurchTimer;
        
        public Vector3 Lurch(Vector3 dir, Vector3 vel, float strength)
        {
            Vector3 initialVel = Vector3.Scale(vel, plane);
            float maxLurchPull = GroundMovementSpeed * strength;
            Vector3 currentDir = initialVel.normalized;
            Vector3 lurchDir = (Vector3.Lerp(currentDir, WishDirection * 1.3f, strength) - currentDir).normalized;
            Vector3 lurchVector = currentDir * initialVel.magnitude + lurchDir * maxLurchPull;
            if (lurchVector.magnitude > initialVel.magnitude)
            {
                lurchVector = lurchVector.normalized * initialVel.magnitude;
            }

            return lurchVector * 0.7f +
                   (Vector3.Scale(vel, plane) * (1 - strength) +
                    (Vector3.Scale(vel, plane) * strength).magnitude * dir) * 0.3f + vel.y * Vector3.up;
        }
        
        private void ResetLurch()
        {
            _lurchTimer = 0;
        }
        #endregion

        #region Air Movement

        [Header("Air Movement")]
        [Tooltip("This is how fast the character can move in the air. It is not a force, but a maximum speed limit.")]
        public float AirMovementSpeed = 15f;
        public float AirAcceleration = 90f;

        private void AirMove(ref Vector3 currentVelocity)
        {
            float deltaTime = Time.deltaTime;
            if (WishDirection.sqrMagnitude > 0f)
            {
                Vector3 addedVelocity = WishDirection * AirAcceleration * deltaTime;

                Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Up);

                // Limit air velocity from inputs
                if (currentVelocityOnInputsPlane.magnitude < AirMovementSpeed)
                {
                    // clamp addedVel to make total vel not exceed max vel on inputs plane
                    Vector3 newTotal =
                        Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, AirMovementSpeed);
                    addedVelocity = newTotal - currentVelocityOnInputsPlane;
                }
                else
                {
                    // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                    if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                }

                // Prevent air-climbing sloped walls
                if (Motor.GroundingStatus.FoundAnyGround)
                    if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                    {
                        Vector3 perpenticularObstructionNormal = Vector3
                            .Cross(Vector3.Cross(Up, Motor.GroundingStatus.GroundNormal),
                                Up).normalized;
                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                    }

                // Apply added velocity
                currentVelocity += addedVelocity;
            }
        }

        private Vector3 AirMove(Vector3 dir, Vector3 vel, float accel, float cap)
        {
            return Accelerate(vel, dir, accel, cap);
        }

        
        #endregion

        #region Jump

        [Header("Jumping")] public float JumpHeight;
        public float DoubleJumpHeight;

        public float doubleJumpLurch;
        private bool _canJump;
        private bool _canDoubleJump;

        
        public void Jump()
        {
        
                Vector3 jumpDirection = RoundVector3(Up, 0);

                if (_canJump)
                {
 
                    
                    // Calculate jump direction before ungrounding
                    // Makes the character skip ground probing/snapping on its next update. 
                    // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                    Motor.ForceUnground();
                    currentDrag = 0;

                    // Add to the return velocity and reset jump state
                    //_velocity.y = 1; 
                    AddVelocity(jumpDirection * JumpHeight);

                    // kill lurch
                    ResetLurch();
                    _canDoubleJump = true;
                }
                else if (_canDoubleJump)
                {
                    // Calculate jump direction before ungrounding
                    // Makes the character skip ground probing/snapping on its next update. 
                    // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                    Motor.ForceUnground();
                    currentDrag = 0;

                    // Add to the return velocity and reset jump state
                    //_velocity.y = 1; 
                    ///Debug.Log($"{Vector3.Dot(_velocity, Up)}");
                    KillDownwardsVelocity();
                    AddVelocity(jumpDirection * DoubleJumpHeight);

                    // kill lurch
                    ResetLurch();
                    _canDoubleJump = false;
                }
            
        }
        #endregion
       
        #region Gravity

        public Vector3 CurrentGravity = new(0, -30f, 0);
        public Vector3 RotationalGravity = new(0, -30f, 0);
        public Vector3 LastGravity = new(0, -30f, 0);
        public Vector3 Gravity = new(0, -30f, 0);

        private void ApplyGravity()
        {
            float deltaTime = Time.fixedDeltaTime;

            LastGravity = CurrentGravity;
            CurrentGravity = Gravity;

            Vector3 position = transform.position;
            position.y -= Motor.Capsule.height * 0.5f;

            Vector3 nearestPlanetGravity = GravityFinder.GetGravity(position);
            if (nearestPlanetGravity != Vector3.zero)
            {
                CurrentGravity = nearestPlanetGravity;
                // this is for spheres
                CurrentBonusOrientationMethod = BonusOrientationMethod.TowardsGroundSlopeAndGravity;
                if (CurrentCharacterState != MotorState.Planet)
                {
                    TransitionToState(MotorState.Planet);
                }
            }
            else
            {
                CurrentGravity = Gravity;
                if (CurrentCharacterState != MotorState.Default)
                {
                    CurrentBonusOrientationMethod = BonusOrientationMethod.TowardsGravity;
                    TransitionToState(MotorState.Default);
                }
            }

            // Still calculate gravity, but return before applying it to avoid the velocity from being affected
            if (!(IsGrounded))
            {
                //Debug.Log($"Currrent velocity: {_velocity}. Velocity after gravity: {CurrentGravity * deltaTime}");
                _velocity += CurrentGravity * deltaTime;
            }
        }

        #endregion
        
        #region Velocity Modification Methods

        private Vector3 _internalVelocityAdd = Vector3.zero;
        private void ApplyVelocityAdd()
        {
            //Debug.Log($"Before {_velocity}  after velocity add: {_internalVelocityAdd}");
            _velocity += _internalVelocityAdd;
            _internalVelocityAdd = Vector3.zero;
        }

        private Vector3 _internalVelocitySet = Vector3.zero;
        private void ApplyVelocitySet()
        {
            _velocity = _internalVelocitySet;
            _internalVelocitySet = Vector3.zero;
        }

        public void AddVelocity(Vector3 velocity)
        {
            switch (CurrentCharacterState)
            {
                case MotorState.Default:
                {
                    _internalVelocityAdd += velocity;
                    break;
                }
                case MotorState.Planet:
                {
                    _internalVelocityAdd += velocity;
                    break;
                }
            }
        }

        private void ForceVelocity(Vector3 velocity)
        {
            switch (CurrentCharacterState)
            {
                case MotorState.Default:
                {
                    _velocity = velocity;
                    break;
                }
                case MotorState.Planet:
                {
                    _velocity = velocity;
                    break;
                }
            }
        }
        
        #endregion

        #region Surface Info

        public SurfaceInfo DefaultSurface;
        [SerializeField] private SurfaceInfo _currentSurface;
        private Collider _currentSurfaceCollider;
        public Collider Surface => _currentSurfaceCollider;

        #endregion

        #region State

        [SerializeField] public MotorState CurrentCharacterState;

        [SerializeField] public MotorState LastCharacterState;

        // { get; private set; }

        public MovementType CurrentMovementType => _currentMovementType;

        [FormerlySerializedAs("currentMovementType")] [SerializeField]
        private MovementType _currentMovementType;

        [SerializeField] private MovementType lastMovementType;

        #endregion

        #region Misc

        [Header("Misc")] public List<Collider> IgnoredColliders = new();
        
        [FormerlySerializedAs("BonusOrientationMethod")] 
        public BonusOrientationMethod CurrentBonusOrientationMethod = BonusOrientationMethod.None;
        
        public float BonusOrientationSharpness = 10f;
            
        private Vector3 _meshTargetScale = Vector3.one;

        private readonly Collider[] _genericColliderCheck = new Collider[8];
        
        // sweephits for the beforecharacterupdate code
        private RaycastHit _sweepHit;
        private RaycastHit[] _sweepHits = new RaycastHit[32];
        
        // normals
        private Vector3 _currentNormal;
        private Vector3 _lateNormal;        

        public Vector3 Up => RoundVector3(transform.rotation * Vector3.up, 2);
        public Vector3 Down => RoundVector3(transform.rotation * Vector3.down, 2);
        public Vector3 Left => transform.rotation * Vector3.left;
        public Vector3 Right => transform.rotation * Vector3.right;
        #endregion

        #region  Velocity

        [SerializeField]
        private Vector3 _velocity;
        public Vector3 Velocity
        {
            get => _velocity;
            set => ForceVelocity(value);
        }


        #endregion




        [SerializeField] 
        private float sameColliderWallrunTimer;
        private float overallWallrunTimer;

        #region Mystery Variables

        private readonly Vector3 plane = new(1, 0, 1);
        public float adhesion = 0.1f;

        #endregion

        #region Drag

        // if you truly loved me, why'd you train me to fight?
        // if it wasn't in my blood, what do you see?

        private float currentDrag;

        #endregion

        #region Input Fields

        /// <summary>
        /// The actual, curated direction the player wants to move in
        /// </summary>
        public Vector3 WishDirection;// { get; private set; }

        /// <summary>
        /// The raw direction the player wants to move in
        /// </summary>
        private Vector2 _rawDirectionalMove;

        // create these first
        private readonly InputEventData _jumpInput = new();
        private readonly InputEventData _crouchInput = new();
        private readonly InputEventData _lurchInput = new();

        #endregion

        #endregion
        public Quaternion Rotation;
        private void Awake()
        {
            Application.targetFrameRate = 500;
            //QualitySettings.vSyncCount = 1;
            Motor.CharacterController = this;

            _cosineMaxSlopeAngle = Mathf.Cos(MaxSlopeAngle * Mathf.Deg2Rad);
            _currentSurface = DefaultSurface;

            // Handle initial state
            TransitionToState(MotorState.Default);
            // Assign the characterController to the motor
        }

        public void SetWishDirection(Vector3 direction) {
            direction = direction.normalized;
            _rawDirectionalMove = new Vector2(direction.x, direction.z);
            WishDirection = new Vector3(direction.x,0, direction.z);
            ; //direction;
        }
        
        public void Update()
        {
            //Debug.Log(Vector3.Dot(Vector3.Scale(plane, _velocity),
            //   Vector3.Scale(WishDirection, plane).normalized));

            // create wish direction
            WishDirection = GetWishDirection();
            //if (Input.GetKey(KeyCode.C)) velocity += _lookInputVector * Time.deltaTime * 50;
            if (IsGrounded)
            {
                _canJump = true;
                if (!_isCrouching) _slideTimer += Time.deltaTime;
            }
            else
            {
                _canJump = false;
            }

            _lurchTimer += Time.deltaTime;

            CheckInput();
            // [i had a funny joke but this code is self-explainatory]
            lastMovementType = _currentMovementType;
            DetermineMovementType();
        }

        public void FixedUpdate()
        {
            sameColliderWallrunTimer -= Time.fixedDeltaTime;
            overallWallrunTimer -= Time.fixedDeltaTime;
            switch (CurrentCharacterState)
            {
                case MotorState.Default:
                {
                    switch (_currentMovementType)
                    {
                        case MovementType.Ground:
                            currentDrag = 0.3f * _currentSurface.Drag;
                            
                            _velocity = GroundMove(WishDirection, _velocity, GroundMovementAcceleration,
                                GroundMovementSpeed);
                            //GroundMove(ref _velocity);
                            break;
                        case MovementType.Air:
                            AirMove(ref _velocity);
                            //_velocity = AirMove(WishDirection, _velocity, AirAcceleration, AirMovementSpeed);

                            //AirMove(ref _velocity);
                            break;
                        case MovementType.Crouching:
                            _velocity = GroundMove(WishDirection, _velocity, CrouchAcceleration, CrouchSpeed);
                            currentDrag = CrouchDrag;
                            break;
                        case MovementType.Deferred:
                            // do nothing,
                            // because velocity is being handled by an outside source
                            break;
                    }

                    // apply gravity out of switch statement
                    ApplyGravity();
                    break;
                }
                case MotorState.Planet:
                    switch (_currentMovementType)
                    {
                        case MovementType.Ground:
                            GroundMove(ref _velocity);
                            break;
                        case MovementType.Air:
                            AirMove(ref _velocity);
                            break;
                        case MovementType.Crouching:
                            _velocity = GroundMove(WishDirection, _velocity, CrouchAcceleration, CrouchSpeed);
                            currentDrag = CrouchDrag * _currentSurface.Drag;
                            break;
                        case MovementType.Deferred:
                            // do nothing,
                            // because velocity is being handled by an outside source
                            break;
                    }

                    ApplyGravity();
                    // Take into account additive velocity
                    break;
            }

            if (_internalVelocityAdd.sqrMagnitude > 0f)
            {
                ApplyVelocityAdd();
            }

            if (_internalVelocitySet.sqrMagnitude > 0f)
            {
                ApplyVelocitySet();
            }
        }

        public void LateUpdate()
        {
            _jumpInput.RefreshKeyData();
            _crouchInput.RefreshKeyData();
            _lurchInput.RefreshKeyData();
        }

        public void DetermineMovementType()
        {
            if (_currentMovementType == MovementType.Deferred)
            {
                return;
            }
            

            
            if (IsGrounded)
            {
                _currentMovementType = MovementType.Ground;
                if (_isCrouching)
                {

                    _currentMovementType = MovementType.Crouching;
                    return;
                }
            }
            else
            {
                _currentMovementType = MovementType.Air;
            }
        }

        
        #region Character States

        /// <summary>
        /// Handles movement state transitions and enter/exit callbacks
        /// </summary>
        public void TransitionToState(MotorState newState)
        {
            MotorState tmpInitialState = CurrentCharacterState;
            RotationalGravity = Gravity;
            OnStateExit(tmpInitialState, newState);
            LastCharacterState = CurrentCharacterState;
            CurrentCharacterState = newState;
            OnStateEnter(newState, tmpInitialState);
        }

        /// <summary>
        /// Event when entering a state
        /// </summary>
        public void OnStateEnter(MotorState state, MotorState fromState)
        {
        }

        /// <summary>
        /// Event when exiting a state
        /// </summary>
        public void OnStateExit(MotorState state, MotorState toState)
        {
        }

        #endregion

        #region Input

        private Vector3 GetWishDirection()
        {
            return WishDirection;
        }

        private void CheckInput()
        {
            if (_crouchInput.pressed)
            {
                /*StreamWriter streamWriter = new StreamWriter("wallrun.log");
                //_wallrunInfos.ForEach(x => streamWriter.WriteLine(x));
                streamWriter.Close();*/

                _shouldBeCrouching = true;

                if (!_isCrouching)
                {
                    _isCrouching = true;
                    Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
                    _meshTargetScale = new Vector3(1, 0.3f, 1);
                }
            }
            else
            {
                _shouldBeCrouching = false;
            }

            if (_lurchInput.down)
            {
                /*if (WishDirection.magnitude > 0 && _currentMovementType == MovementType.Air &&
                    lastMovementType != MovementType.Wallrunning)
                {
                    // we lurching niggaz
                    Vector3 lurchVelocity = Lurch(WishDirection, _velocity,
                        LurchCurve.Evaluate(Mathf.Clamp(_lurchTimer, 0, 1)));
                    ForceVelocity(lurchVelocity);
                }*/
            }
        }

        #endregion
        
        #region Physics!

        /// <summary>
        /// Does source-like acceleration 
        /// </summary>
        /// <param name="currentVelocity">The current velocity</param>
        /// <param name="wishDirection">The direction the player wants to move in.</param>
        /// <param name="accelerationRate">The rate of acceleration.</param>
        /// <param name="accelerationLimit">The limit of acceleration.</param>
        /// <returns>Accelerated velocity</returns>
        private Vector3 Accelerate(Vector3 currentVelocity, Vector3 wishDirection, float accelerationRate,
            float accelerationLimit)
        {
            float speed = Vector3.Dot(Vector3.Scale(plane, currentVelocity),
                Vector3.Scale(wishDirection, plane).normalized);
            float speedGain = accelerationRate * Time.deltaTime;

            if (speed + speedGain > accelerationLimit)
                speedGain = Mathf.Clamp(accelerationLimit - speed, 0, accelerationLimit);

            return currentVelocity + wishDirection * speedGain;
        }

        /// <summary>
        /// Does source-like acceleration 
        /// </summary>
        /// <param name="currentVelocity">The current velocity</param>
        /// <param name="wishDirection">The direction the player wants to move in.</param>
        /// <param name="accelerationRate">The rate of acceleration.</param>
        /// <param name="accelerationLimit">The limit of acceleration.</param>
        /// <returns>Accelerated velocity</returns>
        private Vector3 AccelerateUnlimited(Vector3 currentVelocity, Vector3 wishDirection, float accelerationRate)
        {
            float speedGain = accelerationRate * Time.deltaTime;
            return currentVelocity + wishDirection * speedGain;
        }

        
        
        private Vector3 PlanarAccelerate(Vector3 currentVelocity, Vector3 wishDirection, float accelerationRate,
            float accelerationLimit)
        {
            float speed = Vector3.Dot(Vector3.Scale(Motor.GroundingStatus.GroundNormal, currentVelocity),
                Vector3.Scale(wishDirection,  Motor.GroundingStatus.GroundNormal).normalized);
            float speedGain = accelerationRate * Time.deltaTime;

            if (speed + speedGain > accelerationLimit)
                speedGain = Mathf.Clamp(accelerationLimit - speed, 0, accelerationLimit);

            return currentVelocity + wishDirection * speedGain;
        }

        /// <summary>
        /// Applies friction
        /// </summary>
        /// <param name="currentVelocity">The velocity to apply the friction to</param>
        /// <param name="friction">The amount of friction to apply</param>
        /// <returns></returns>
        public Vector3 ApplyFriction(Vector3 currentVelocity, float friction)
        {
            return currentVelocity * (1 / (friction + 1));
        }

        #endregion

        #region General Movement


        /// <summary>
        /// When the player hits something, this method is called to calculate the velocity the player should have upon impact.
        /// </summary>
        private void CalculateOnHitVelocity(Vector3 _currentNormalA)
        {
            bool fall = false;
            if (lastMovementType == MovementType.Air && _currentNormalA.y > 0.706f) fall = true;
            if (!fall)
            {
                float momentum = Vector3.Dot(_currentNormal, _velocity);
                _velocity -= _currentNormalA * momentum;
                _velocity -= _currentNormalA * adhesion;
            }
            else
            {
                Vector3 startVel = _velocity;
                float momentum = Vector3.Dot(Up, _velocity);
                _velocity -= Up * momentum;

                Vector3 dir = Vector3.zero;
                dir = Vector3.Cross(new Vector3(_velocity.z, 0, -_velocity.x).normalized, _currentNormalA);
                _velocity = dir * _velocity.magnitude;
              
                if (Vector3.Dot(_velocity, _currentNormalA) > 0.1f)
                {
                    _velocity = startVel;
                    momentum = Vector3.Dot(_currentNormalA, _velocity);
                    _velocity -= _currentNormalA * momentum;
                }
            }
        }

        #endregion
        
        #region Built in Kinematic Character Motor methods

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its rotation should be right now. 
        /// This is the ONLY place where you should set the character's rotation
        /// </summary>
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case MotorState.Default:
                {
                    
                    currentRotation = Rotation;
                    
                    if (CurrentBonusOrientationMethod == BonusOrientationMethod.TowardsGravity)
                    {
                        // Rotate from current up to invert gravity
                        Vector3 smoothedGravityDir = Vector3.Slerp(Up, -CurrentGravity.normalized,
                            1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                        currentRotation = Quaternion.FromToRotation(Up, smoothedGravityDir) * currentRotation;
                    }
                    else if (CurrentBonusOrientationMethod == BonusOrientationMethod.TowardsGroundSlopeAndGravity)
                    {
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            Vector3 initialCharacterBottomHemiCenter =
                                Motor.TransientPosition + Up * Motor.Capsule.radius;


                            Vector3 smoothedGroundNormal = Vector3.Slerp(Up,
                                Motor.GroundingStatus.GroundNormal,
                                1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            currentRotation = Quaternion.FromToRotation(Up, smoothedGroundNormal) *
                                              currentRotation;

                            
                            /* Move the position to create a rotation around the bottom hemi center instead of around the pivot
                            Motor.SetTransientPosition(initialCharacterBottomHemiCenter +
                                                       (currentRotation * Vector3.down * Motor.Capsule.radius));*/
                        }
                        else
                        {
                            Vector3 smoothedGravityDir = Vector3.Slerp(Up, -CurrentGravity.normalized,
                                1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            currentRotation = Quaternion.FromToRotation(Up, smoothedGravityDir) *
                                              currentRotation;
                        }
                    }
                    else
                    {
                        Vector3 smoothedGravityDir = Vector3.Slerp(Up, Vector3.up,
                            1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                        currentRotation = Quaternion.FromToRotation(Up, smoothedGravityDir) * currentRotation;
                    }

                    break;
                }
                case MotorState.Planet:

                    Vector3 currentUpP = currentRotation * Vector3.up;

                    if (CurrentBonusOrientationMethod == BonusOrientationMethod.TowardsGravity)
                    {
                        // Rotate from current up to invert gravity
                        Vector3 smoothedGravityDir = Vector3.Slerp(currentUpP, -CurrentGravity.normalized,
                            1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                        currentRotation = Quaternion.FromToRotation(currentUpP, smoothedGravityDir) * currentRotation;
                    }
                    else if (CurrentBonusOrientationMethod == BonusOrientationMethod.TowardsGroundSlopeAndGravity)
                    {
                        Quaternion rotation = Quaternion.identity;
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            Vector3 initialCharacterBottomHemiCenter =
                                Motor.TransientPosition + currentUpP * Motor.Capsule.radius;


                            Vector3 smoothedGroundNormal = Vector3.Slerp(Up,
                                Motor.GroundingStatus.GroundNormal,
                                1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            rotation = Quaternion.FromToRotation(currentUpP, smoothedGroundNormal) *
                                       currentRotation;

                            /* Move the position to create a rotation around the bottom hemi center instead of around the pivot
                            Motor.SetTransientPosition(initialCharacterBottomHemiCenter +
                                                       (currentRotation * Vector3.down * Motor.Capsule.radius));*/
                        }
                        else
                        {
                            Vector3 smoothedGravityDir = Vector3.Slerp(currentUpP, -CurrentGravity.normalized,
                                1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));

                            rotation = Quaternion.FromToRotation(currentUpP, smoothedGravityDir) *
                                       currentRotation;
                        }

                        currentRotation = rotation;
                    }
                    else
                    {
                        Vector3 smoothedGravityDir = Vector3.Slerp(currentUpP, Vector3.up,
                            1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                        currentRotation = Quaternion.FromToRotation(currentUpP, smoothedGravityDir) * currentRotation;
                    }

                    break;
            }
        }


        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its velocity should be right now. 
        /// This is the ONLY place where you can set the character's velocity
        /// </summary>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // Here, we're just going to set the velocity to be the desired velocity we keep in this character motor
            // For two reasons:
            // 1) We want to be able to control the character's velocity in any way we want
            // 2) I am not shoving all of my movement code here. Fuck you.
            currentVelocity = _velocity;
            _velocity = currentVelocity;     

            Vector3 currentUpD = Motor.TransientRotation * Vector3.up;
            DrawVector(Motor.TransientPosition, Up, 25, Color.red);
            DrawVector(Motor.TransientPosition, -Up, 25, Color.blue);
        }

        #endregion

        #region Character Update Methods

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called before the character begins its movement update
        /// Called in Fixed Update
        /// </summary>
        public void BeforeCharacterUpdate(float deltaTime)
        {
            if (RootMotion != Vector3.zero)
            {
                Vector3 storage = RootMotion;
                RootMotion = Vector3.zero;
                Motor.MoveCharacter(base.transform.position + storage);
            }
            

            // This is used to determine if the character is colliding with anything
            // If they aren't, wallrunning is stopped
            int hitShit = Motor.CharacterCollisionsSweep(Motor.TransientPosition, Motor.TransientRotation,
                _velocity.normalized, 0.1f, out _sweepHit, _sweepHits, 0f);
            DrawVector(Motor.TransientPosition, _velocity, 25, Color.green);
            
            if (hitShit > 0)
            {
                bool newSurface = false;
                if (_sweepHit.collider != _currentSurfaceCollider)
                {
                    newSurface = true;
                    SurfaceInfo info = SurfaceInfo.FindSurfaceInfo(_sweepHit.collider);
                    if (info != null)
                    {
                        _currentSurface = info;
                    }
                    else
                    {
                        _currentSurface = DefaultSurface;
                    }

                    _currentSurfaceCollider = _sweepHit.collider;
                }

                _lateNormal = _currentNormal;
                _currentNormal = _sweepHit.normal;
            }
        }

        public Vector3 RoundVector3(Vector3 vector, int decimals = 1)
        {
            vector.x = (float) Math.Round(vector.x, decimals);
            vector.y = (float) Math.Round(vector.y, decimals);
            vector.z = (float) Math.Round(vector.z, decimals);
            return vector;
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called after the character has finished its movement update.
        /// Called in Fixed Update
        /// </summary>
        public void AfterCharacterUpdate(float deltaTime)
        {
            //Debug.Log(Motor.CharacterCollisionsOverlap(base.transform.position, base.transform.rotation, hits)) ;


            switch (CurrentCharacterState)
            {
                case MotorState.Default:
                {
                    if (_isCrouching && !_shouldBeCrouching)
                    {
                        // Do an overlap test with the character's standing height to see if there are any obstructions
                        Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
                        if (Motor.CharacterOverlap(
                                Motor.TransientPosition,
                                Motor.TransientRotation,
                                _genericColliderCheck,
                                Motor.CollidableLayers,
                                QueryTriggerInteraction.Ignore) > 0)
                        {
                            // If obstructions, just stick to crouching dimensions
                            Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
                            _meshTargetScale = new Vector3(1, 0.3f, 1);
                        }
                        else
                        {
                            // If no obstructions, uncrouch
                            _meshTargetScale = Vector3.one;
                            _isCrouching = false;
                        }
                    }


                    break;
                }
                case MotorState.Planet:
                    // Handle uncrouching
                    if (_isCrouching && !_shouldBeCrouching)
                    {
                        // Do an overlap test with the character's standing height to see if there are any obstructions
                        Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
                        if (Motor.CharacterOverlap(
                                Motor.TransientPosition,
                                Motor.TransientRotation,
                                _genericColliderCheck,
                                Motor.CollidableLayers,
                                QueryTriggerInteraction.Ignore) > 0)
                            // If obstructions, just stick to crouching dimensions
                            Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
                        else
                        {
                            // If no obstructions, uncrouch
                            _meshTargetScale = Vector3.one;
                            _isCrouching = false;
                        }
                    }

                    break;
            }
        }

        #endregion


        #region Grounding & Movement Methods
        
        public void PostGroundingUpdate(float deltaTime)
        {
            // Handle landing and leaving ground
            if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
                OnLanded();
            else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
                OnLeaveStableGround();
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            if (IgnoredColliders.Count == 0) return true;

            if (IgnoredColliders.Contains(coll)) return false;

            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
        }

        private Vector3 Flatten(Vector3 vec)
        {
            return KillDownwardsVelocity(vec); // new Vector3(vec.x, 0f, vec.z);
        }

        public void KillDownwardsVelocity()
        {
            float momentum = Vector3.Dot(Up, _velocity);
            _velocity -= Up * momentum;
        }
        
        public Vector3 KillDownwardsVelocity(Vector3 vector)
        {
            float momentum = Vector3.Dot(Up, vector);
            return vector -= Up * momentum;
        }
        
        public Vector3 KillUpwardsVelocity(Vector3 vector)
        {
            float momentum = Vector3.Dot(Down, vector);
            return vector -= Down * momentum;
        }       
        private bool isVelocityCancelled;
        private bool lateVelocityCancel;

        


        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
            lateVelocityCancel = false;
            

            
            if (isVelocityCancelled && !lateVelocityCancel) CalculateOnHitVelocity(hitNormal);
            lateVelocityCancel = isVelocityCancelled;
          
            isVelocityCancelled = true;
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
            // kanye west
            _currentSurfaceCollider = hitCollider;
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
            
        }

        public event Action OnGrounded;
        protected void OnLanded()
        {
            if (CurrentCharacterState == MotorState.Default)
            {
                CurrentGravity = Gravity;
            }
            // incur a small penalty
            _velocity -= _velocity * 0.23f;

            _currentSurfaceCollider = Motor.GroundingStatus.GroundCollider;
            OnGrounded?.Invoke();
            //Debug.Log("Movement penalty incurred");
        }

        protected void OnLeaveStableGround()
        {
            _canDoubleJump = true;
        }

        ///<summary> draws Vector gizmo, only for debugging <summary>
        public void DrawVector(Vector3 origin, Vector3 vector, float lengthMultiplier, Color color)
        {
            Debug.DrawLine(origin, origin + vector * lengthMultiplier, color);
        }

        //private List<string> _wallrunInfos= new List<string>();

        #endregion
    }
}