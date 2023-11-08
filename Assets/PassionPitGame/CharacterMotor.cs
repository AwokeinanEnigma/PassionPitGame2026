#region

using Enigmaware.World;
using KinematicCharacterController;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

#endregion

namespace PassionPitGame
{



    public class CharacterMotor : MonoBehaviour, ICharacterController
    {
        public Transform CameraPosition;
        float _motionInterpolationDelta;

        Vector3 _previousPosition;
        public Vector3 InterpolatedPosition => Vector3.Lerp(_previousPosition, CameraPosition.position,
            _motionInterpolationDelta / Time.fixedDeltaTime);

        void Awake()
        {
            _input = FindObjectOfType<PlayerInput>();
            
            Motor.CharacterController = this;

            _cosineMaxSlopeAngle = Mathf.Cos(MaxSlopeAngle * Mathf.Deg2Rad);
            
            CurrentSurface = DefaultSurface;

            // Handle initial state
            TransitionToState(MotorState.Default);
        }

        public void Update()
        {
            Application.targetFrameRate = 500;
            HandleCamera();
            
            _motionInterpolationDelta += Time.deltaTime;

            // create wish direction
            WishDirection = GetWishDirection();
            //if (Input.GetKey(KeyCode.C)) velocity += _lookInputVector * Time.deltaTime * 50;
            if (IsGrounded)
            {
                _canJump = true;
            }
            else
            {
                _canJump = false;
            }


            CheckInput();

            MeshRoot.localScale =
                Vector3.Lerp(MeshRoot.localScale, _meshTargetScale, Time.deltaTime * CameraCrouchSpeed);

            
            // [i had a funny joke but this code is self-explainatory]
            _lastMovementType = _currentMovementType;
            DetermineMovementType();
        }
        public void FixedUpdate()
        {        
            jumpBuffered = Mathf.Max(jumpBuffered - Time.fixedDeltaTime, 0);
            eatJumpInputs = Mathf.Max(eatJumpInputs - Time.fixedDeltaTime, 0);
            _motionInterpolationDelta = 0;
            
            switch (CurrentCharacterState)
            {
                case MotorState.Default:
                {
                    switch (_currentMovementType)
                    {
                        case MovementType.Ground:
                            
                            
                            _currentDrag = 0.3f * CurrentSurface.Drag;
                            //  if (!_isStepping) _velocity.y = 0;
                            GroundMove(WishDirection, _velocity, GroundMovementAcceleration,
                                GroundMovementSpeed);

                            //GroundMove(ref _velocity);
                            break;
                        case MovementType.Air:
                            AirMove(ref _velocity);
                            //_velocity = AirMove(WishDirection, _velocity, AirAcceleration, AirMovementSpeed);

                            //AirMove(ref _velocity);
                            break;
                        case MovementType.Sliding:
                            _currentDrag = SlideDrag; // * movementSurface.dragMultiplier;
                             GroundMove(WishDirection, _velocity, CrouchAcceleration, CrouchSpeed);
                            Vector3 slideDir =
                                Vector3.down - Vector3.Dot(_currentNormal, Vector3.down) * _currentNormal;

                            _velocity += slideDir * SlideAcceleration;
                            CameraController.SetFOV(110 + _velocity.magnitude / 3);
                                                                                                                                                break;
                        case MovementType.Crouching:
                            _currentDrag = CrouchDrag;
                             GroundMove(WishDirection, _velocity, CrouchAcceleration, CrouchSpeed);
                            break;
                        case MovementType.Deferred:
                            // do nothing,
                            // because velocity is being handled by an outside source
                            break;
                    }
                    // apply gravity out of switch statement
                    if (_currentMovementType != MovementType.Deferred)
                    {
                        ApplyGravity();
                    }
                    //ApplyGravity();
                    break;
                }
            }
            if (_currentMovementType != MovementType.Sliding)// && _currentMovementType != MovementType.RailGrinding)
            {
                CameraController.SetFOV(110);
            }
        }

        public float CalculateYForDirection(Vector3 direction, float max = 100f)
        {
            return CalculateYForDirectionAndSpeed(direction, Speed, max);
        }

        public float CalculateYForDirectionAndSpeed(Vector3 direction, float speed, float max = 100f)
        {
            var wishdir = direction.normalized;
            var x2 = Flatten(wishdir).magnitude;
            var x1 = speed;
            var y2 = wishdir.y;
            var y1 = x1 * y2 / x2;

            if (Mathf.Abs(y1) > max) y1 = Mathf.Sign(y1) * max;
            return y1;
        }
        
        public void LateUpdate()
        {
            _jumpInput.RefreshKeyData();
            _crouchInput.RefreshKeyData();
            _lurchInput.RefreshKeyData();
        }

        #region Movement Types

        public void ForceMovementType(MovementType type)
        {
            _lastMovementType = _currentMovementType;
            _currentMovementType = type;
        }

        #endregion

        void DetermineMovementType()
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
                    if (_velocity.magnitude > SlideMinimumMagnitude + 1)
                    {
                        _currentMovementType = MovementType.Sliding;
                        return;
                    }
                    _currentMovementType = MovementType.Crouching;
                }
            }
            else
            {
                _currentMovementType = MovementType.Air;
            }
        }

        #region General Movement

        /// <summary>
        /// When the player hits something, this method is called to calculate the velocity the player should have upon impact.
        /// </summary>
         void CalculateOnHitVelocity(Vector3 _currentNormalA)
        {
            Debug.Log("calculating on hit");
            bool fall = _lastMovementType == MovementType.Air && _currentNormalA.y > 0.706f;
            if (!fall)
            {
                float momentum = Vector3.Dot(_currentNormal, _velocity);
                _velocity -= _currentNormalA * momentum;
                _velocity -= _currentNormalA * Adhesion;
            }
            else
            {
                Vector3 startVel = _velocity;
                float momentum = Vector3.Dot(Vector3.up, _velocity);
                _velocity -= Vector3.up * momentum;

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
        #region  Enums

        public enum MotorState
        {
            Default,
        }

        public enum MovementType
        {
            // On the ground movement
            Ground,

            // In the air movement!
            Air,

            // Like sliding, but less cool.
            Crouching,

            // Sliding
            Sliding,
            
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

        #region Fields

        public Vector3 RootMotion;
        public KinematicCharacterMotor Motor;
        [FormerlySerializedAs("_cameraController")]
        [SerializeField]
         CameraController CameraController;
         public Transform MeshRoot;
         bool _isStepping;

         #region Ground Movement

         float _cosineMaxSlopeAngle;

         [Header("Stable Movement")]
        public float GroundMovementSpeed = 15f;
        public float GroundMovementAcceleration = 200f;
        public float MaxSlopeAngle = 50f;
        public int Coyote_Ticks = 5;

        [Header("Crouching")]
         bool _shouldBeCrouching;
         bool _isCrouching;

         public float CrouchedCapsuleHeight = 1f;
         public float CrouchSpeed;
         public float CrouchDrag;
         public float CrouchAcceleration;
         public float CameraCrouchSpeed;

         public bool IsGrounded => Motor.GroundingStatus.IsStableOnGround;
         public float Speed {
             get {
                 var flat = Flatten(_velocity);
                 if (Math.Abs(Mathf.Pow(speed, 2) - flat.sqrMagnitude) > 0.01f) {
                     speed = flat.magnitude;
                 }

                 return speed;
             }
             set {
                 var y = _velocity.y;
                 _velocity = Flatten(_velocity).normalized*value;
                 _velocity.y = y;
             }	
         }
         float speed;
         
         /// <summary>
        /// Used for the default state for when we're not on a planet
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="vel"></param>
        /// <param name="accel"></param>
        /// <param name="cap"></param>
        /// <returns></returns>
        void GroundMove(Vector3 dir, Vector3 vel, float accel, float cap)
        {
            vel = ApplyFriction(vel, _currentDrag);
            groundTimestamp = _input.tickCount;
            _velocity = Accelerate(vel, dir, accel, cap); //Accelerate(vel, dir, accel, cap);
            PlayerJump();
        }

         #endregion

         #region Sliding

         [Header("Sliding")] 
        public float SlideMinimumMagnitude;
        public float SlideDrag;
        public float SlideAcceleration;

        #endregion

        #region Lurch

        public void Lurch(Vector3 direction, float strength = 0.7f)
        {
            var wishdir = Flatten(direction).normalized;

            var max = Mathf.Min(AirMovementSpeed, Speed) * 0.8f;

            var lurchdirection = Flatten(_velocity) + wishdir * max;
            var strengthdirection = Vector3.Lerp(Flatten(_velocity), lurchdirection, strength);

            var resultspeed = Mathf.Min(Vector3.Dot(strengthdirection.normalized, Flatten(_velocity)),
                lurchdirection.magnitude);

            var lurch = strengthdirection.normalized * resultspeed;
            _velocity.x = lurch.x;
            _velocity.z = lurch.z;
        }

        #endregion

        #region Air Movement

        [Header("Air Movement")]
        [Tooltip("This is how fast the character can move in the air. It is not a force, but a maximum speed limit.")]
        public float AirMovementSpeed = 15f;
        public float AirAcceleration = 90f;

        RaycastHit _groundApproachHit;
        RaycastHit[] _otherGroundApproachHits = new RaycastHit[8];

        void AirMove(ref Vector3 currentVelocity) {
            //currentVelocity= Accelerate(currentVelocity, WishDirection, AirAcceleration, AirMovementSpeed);

            bool eatJump = false;
            // Lean in
            var movement = _velocity;
            var didHit = Motor.CharacterCollisionsSweep(Motor.TransientPosition, Motor.TransientRotation,movement.normalized, movement.magnitude * 0.25F, out _groundApproachHit,_otherGroundApproachHits) > 0;
            if (didHit && Motor.IsStableOnNormal(_groundApproachHit.normal) && IsColliderValidForCollisions(_groundApproachHit.collider))
            {
                // Eat jump inputs if you are < jumpForgiveness away from the ground to not eat double jump
                if (_groundApproachHit.distance / movement.magnitude / Time.fixedDeltaTime < 5)
                {
                    eatJump = true; 
                }
            }

            AirAccelerate(ref _velocity, Time.fixedDeltaTime);

            // If we're eating jump inputs, dont check for PlayerJump()
            // Also set jumpBuffered higher than needed so that when PlayerJump() does eventually run it'll use the buffer
            if ((eatJump || jumpBuffered > 0))
            {
                if (_input.SincePressed(PlayerInput.Jump) == 0)
                {
                    jumpBuffered = 4 + Time.fixedDeltaTime * 2;
                }
            }
            else PlayerJump();
        
        }
        const float AIR_SPEED = 2.4f;
        const float SIDE_AIR_ACCELERATION = 60;
        const float FORWARD_AIR_ACCELERATION = 75;
        const float DIAGONAL_AIR_ACCEL_BONUS = 30;
        const float BACKWARD_AIR_ACCELERATION = 35;
        public void AirAccelerate (ref Vector3 vel, float f, float accelMod = 1, float sideairspeedmod = 1f) {
            if (Speed < 10/2) {
                Accelerate(WishDirection, 10, 11/3, f);
                return;
            }

            var forward = transform.forward*_input.GetAxisStrafeForward();
            var right = transform.right*_input.GetAxisStrafeRight();

            var accel = FORWARD_AIR_ACCELERATION*accelMod;

            // Different acceleration for holding backwards lets me have high accel for air movement without
            // pressing s slamming you to a full stop
            if (Vector3.Dot(Flatten(vel), forward) < 0) {
                accel = BACKWARD_AIR_ACCELERATION*accelMod;
            }

            // Player can turn sharper if holding forward and proper side direction
            if (_input.GetAxisStrafeRight() != 0 && _input.GetAxisStrafeForward() > 0 &&
                Vector3.Dot(right, Flatten(vel)) < 0) {

                accel += DIAGONAL_AIR_ACCEL_BONUS;
                var speed = Flatten(vel).magnitude;
                vel += WishDirection*accel*f;
                if (speed < Flatten(vel).magnitude) {
                    var y = vel.y;
                    vel = Flatten(vel).normalized*speed;
                    vel.y = y;
                }

            } else {
                var speed = Flatten(vel).magnitude;
                vel += forward*accel*f;
                if (speed < Flatten(vel).magnitude) {
                    var y = vel.y;
                    vel = Flatten(vel).normalized*speed;
                    vel.y = y;
                }
            }


            if (_input.GetAxisStrafeRight() != 0 && (_input.GetAxisStrafeForward() <= 0)) {
                var sideaccel = SIDE_AIR_ACCELERATION*accelMod;
                var airspeed = AIR_SPEED*sideairspeedmod;

                // Bonus side accel makes surfing more responsive
                // This bonus persists for a bit after leaving a surf so you can actually jump off ramps
                // (also leaves some cool high level tech potential for slant boosts)

                // Air strafing has an offset applied to it so it always pushes you to go straight forward regardless of air speed
                var offset = vel + right*airspeed;
                var angle = Mathf.Atan2(offset.z, offset.x) - Mathf.Atan2(vel.z, vel.x);

                var offsetAngle = Mathf.Atan2(right.z, right.x) - angle;
                right = new Vector3(Mathf.Cos(offsetAngle), 0, Mathf.Sin(offsetAngle));

                // This is just source air strafing
                var rightspeed = Vector3.Dot(vel, right);
                var rightaddspeed = Mathf.Abs(airspeed) - rightspeed;
                if (rightaddspeed > 0) {
                    if (sideaccel*f > rightaddspeed)
                        sideaccel = rightaddspeed/f;

                    var addvector = sideaccel*right;
                    vel += addvector*f;
                }
            }
        }
        // Returns speed gain
        public float Accelerate (Vector3 wishdir, float speed, float acceleration, float f = 1, bool hardCap = false) {
            var beforeSpeed = Speed;
            var currentspeed = Vector3.Dot(_velocity, wishdir.normalized);
            var addspeed = Mathf.Abs(speed) - currentspeed;

            if (addspeed <= 0)
                return 0f;

            var accelspeed = acceleration*f*speed;
            if (accelspeed > addspeed)
                accelspeed = addspeed;
		
            _velocity += accelspeed*wishdir;

            if (Speed > speed) {
                Speed = Mathf.Min(beforeSpeed, Speed);
            }

            return accelspeed;
        }

        #endregion

        #region Jump

        [Header("Jumping")] public float JumpHeight;
        public float DoubleJumpHeight;

        bool _canJump;
        public bool CanDoubleJump;

        private float jumpBuffered;
        private float eatJumpInputs;

        public bool PlayerJump()
    {
        int sinceJump = _input.SincePressed(PlayerInput.Jump);
        if (sinceJump <= 4 || jumpBuffered > 0)
        {
            if (eatJumpInputs > 0)
            {
                _input.ConsumeBuffer(PlayerInput.Jump);
                return true;
            }
            
            var groundJump = _input.tickCount - groundTimestamp < Coyote_Ticks;

            groundTimestamp = -10000;

            Motor.ForceUnground();

            if (!groundJump) {
                if (CanDoubleJump) {
                    CanDoubleJump = false;
                    _velocity.y = Mathf.Max(DoubleJumpHeight, _velocity.y);
                    // Apply a lurch and give a bit of speed if youre below a certain speed
                    // Good for when players make big mistakes and can use double jump to recover from very low speeds in air
                    var speed = Speed;
                    var strength = Mathf.Clamp01((1 - speed/GroundMovementSpeed)*4);
                    Lurch(WishDirection, strength);
                    var doubleJumpSpeed = GroundMovementSpeed/1.5f;
                    if (Speed < doubleJumpSpeed)
                        _velocity += WishDirection*(doubleJumpSpeed - Speed);
                }
            } else
            {
                var height = JumpHeight;
                if (_velocity.y > 0) height += _velocity.y;

                float beforeY = _velocity.y;
                _velocity.y = JumpHeight;
            }


            _input.ConsumeBuffer(PlayerInput.Jump);
            jumpBuffered = 0;
            return true;
        }

        return false;
    }

        #endregion

        #region Gravity

        public Vector3 CurrentGravity = new Vector3(0, -30f, 0);
        public Vector3 RotationalGravity = new Vector3(0, -30f, 0);
        public Vector3 LastGravity = new Vector3(0, -30f, 0);
        public Vector3 Gravity = new Vector3(0, -30f, 0);

        void ApplyGravity()
        {
            float deltaTime = Time.fixedDeltaTime;

            LastGravity = CurrentGravity;
            CurrentGravity = Gravity;
            

            // Still calculate gravity, but return before applying it to avoid the velocity from being affected
            if (!(IsGrounded))
            {
                //Debug.Log($"Currrent velocity: {_velocity}. Velocity after gravity: {CurrentGravity * deltaTime}");
                _velocity += CurrentGravity * deltaTime;
            }
        }

        #endregion

        #region Surface Info

        public SurfaceInfo DefaultSurface;
        public SurfaceInfo CurrentSurface { get;  set; }
        Collider _currentSurfaceCollider;

        public Collider Surface => _currentSurfaceCollider;

        #endregion

        #region State

        [SerializeField] public MotorState CurrentCharacterState;

        [SerializeField] public MotorState LastCharacterState;

        // { get;  set; }

        public MovementType CurrentMovementType => _currentMovementType;

        [FormerlySerializedAs("currentMovementType")] [SerializeField] 
        MovementType _currentMovementType;
        [FormerlySerializedAs("lastMovementType")]
        [SerializeField]
        MovementType _lastMovementType;

        #endregion

        #region Misc

        [Header("Misc")] public List<Collider> IgnoredColliders = new();

        [FormerlySerializedAs("BonusOrientationMethod")] 
        public BonusOrientationMethod CurrentBonusOrientationMethod = BonusOrientationMethod.None;

        public float BonusOrientationSharpness = 10f;

        Vector3 _meshTargetScale = Vector3.one;

        readonly Collider[] _genericColliderCheck = new Collider[8];


        // normals
        Vector3 _currentNormal;
        Vector3 _lateNormal;

        #endregion

        #region  Velocity

        [SerializeField]
         Vector3 _velocity;
         public Vector3 Velocity
        {
            get => _velocity;
            set => _velocity = value;
        }

         #endregion


         #region Mystery Variables

         readonly Vector3 _plane = new Vector3(1, 0, 1);
         [FormerlySerializedAs("adhesion")]
        public float Adhesion = 0.1f;

        #endregion

        #region Drag

        // if you truly loved me, why'd you train me to fight?
        // if it wasn't in my blood, what do you see?

        float _currentDrag;

        #endregion

        #region Input Fields

        /// <summary>
        /// The actual, curated direction the player wants to move in
        /// </summary>
        public Vector3 WishDirection { get;  set; }

        /// <summary>
        /// The raw direction the player wants to move in
        /// </summary>
         Vector2 _rawDirectionalMove;

        // create these first
        readonly InputEventData _jumpInput = new InputEventData();
        readonly InputEventData _crouchInput = new InputEventData();
        readonly InputEventData _lurchInput = new InputEventData();

        [FormerlySerializedAs("Input")]
        PlayerInput _input;

        #endregion

        #region Timestamps

        // This is used for input buffering
        private int groundTimestamp = -100000;

        #endregion

        #endregion

        #region CameraHandling

        public float Yaw { get; set; }
        public float YawFutureInterpolation { get; set; }
        public float YawIncrease { get; set; }
        public float Pitch { get; set; }
        public float PitchFutureInterpolation { get; set; }
        public float LookScale = 1;
        public float CameraRoll { get; set; }

        private float velocityThunk;
        private float velocityThunkSmoothed;
        private float previousYVelocity;

        public const float VIEWBOBBING_SPEED = 8;
        private float viewBobbingAmount;
        private float threeSixtyCounter;

        private float cameraRotation;
        private float cameraRotationSpeed;

        public Camera Camera;

        public void SetCameraRoll(float target, float speed)
         {
             cameraRotation = target;
             // Cap tilt speed to prevent camera tilt being too snappy
             if (speed > 150) speed = 150;
             cameraRotationSpeed = speed;
         }

        void HandleCamera () {
             if (Time.timeScale > 0) {
                 YawIncrease = Input.GetAxis("Mouse X")*(1f/10)*LookScale;
                 YawIncrease += Input.GetAxis("Joy 1 X 2")*1*LookScale;

                 Yaw = (Yaw + YawIncrease)%360f;


                 var yawinterpolation = Mathf.Lerp(Yaw, Yaw + YawFutureInterpolation, Time.deltaTime*10) - Yaw;
                 Yaw += yawinterpolation;
                 YawFutureInterpolation -= yawinterpolation;

                 Pitch -= Input.GetAxis("Mouse Y")*(1f/10)*LookScale;
                 Pitch += Input.GetAxis("Joy 1 Y 2")*1f*LookScale;
                 
             var pitchinterpolation = Mathf.Lerp(Pitch, Pitch + PitchFutureInterpolation, Time.deltaTime*10) - Pitch;
                 Pitch += pitchinterpolation;
                 PitchFutureInterpolation -= pitchinterpolation;

                 Pitch = Mathf.Clamp(Pitch, -85, 85);
             }

             threeSixtyCounter -= Mathf.Min(threeSixtyCounter, Time.deltaTime*150);
             threeSixtyCounter += Mathf.Abs(YawIncrease);
             if (threeSixtyCounter > 240) {
                 threeSixtyCounter -= 240;
             }

             // This is where orientation is handled, the Camera is only adjusted by the pitch, and the entire player is adjusted by yaw
             velocityThunk = Mathf.Lerp(velocityThunk, 0, Time.deltaTime*4);
             velocityThunkSmoothed = Mathf.Lerp(velocityThunkSmoothed, velocityThunk, Time.deltaTime*16);
             
             velocityThunk += (_velocity.y - previousYVelocity)/3f;
             previousYVelocity = _velocity.y;

             viewBobbingAmount -= Mathf.Min(Time.deltaTime*3, viewBobbingAmount);
             var yawBobbing = (Mathf.Sin((Time.time*VIEWBOBBING_SPEED) + Mathf.PI/2) - 0.5f)*viewBobbingAmount*0.6f;
             var pitchBobbing = (Mathf.Abs(Mathf.Sin(Time.time*VIEWBOBBING_SPEED)) - 0.5f)*viewBobbingAmount*0.4f;

             CameraController.transform.localRotation =
                 Quaternion.Euler(new Vector3(Pitch + velocityThunkSmoothed - pitchBobbing, 0, CameraRoll));
             Rotation = Quaternion.Euler(0, Yaw + yawBobbing, 0);

             // This value is used to calcuate the positions in between each fixedupdate tick

             CameraRoll -= Mathf.Sign(CameraRoll - cameraRotation)*Mathf.Min(cameraRotationSpeed*Time.deltaTime,
                 Mathf.Abs(CameraRoll - cameraRotation));
             Camera.transform.position = InterpolatedPosition;
         }

        public Quaternion Rotation;

        #endregion


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
        void OnStateEnter(MotorState state, MotorState fromState)
        {
        }

        /// <summary>
        /// Event when exiting a state
        /// </summary>
        void OnStateExit(MotorState state, MotorState toState)
        {
        }

        #endregion

        #region Input

        Vector3 GetWishDirection()
        {
            switch (CurrentCharacterState)
            {
                case MotorState.Default:
                    if (!IsGrounded || _currentNormal.y < _cosineMaxSlopeAngle)
                        return (_rawDirectionalMove.x * CameraController.PlanarRight +
                                _rawDirectionalMove.y * CameraController.PlanarForward).normalized;
                    return Vector3
                        .Cross(
                            (_rawDirectionalMove.x * -CameraController.PlanarForward +
                             _rawDirectionalMove.y * CameraController.PlanarRight).normalized,
                            _currentNormal).normalized;
                
            }

            return Vector3.zero;
        }

        // These methods are called by Unity
        // Don't mess with them unless you know what you're doing!
        public void ReadMovement(InputAction.CallbackContext context)
        {
            // swagilicous
            _rawDirectionalMove = context.ReadValue<Vector2>();
        }

        public void ReadJump(InputAction.CallbackContext context)
        {
            _jumpInput.UpdateKeyState(context);
        }

        public void ReadCrouch(InputAction.CallbackContext context)
        {
            _crouchInput.UpdateKeyState(context);
        }

        public void ReadLurch(InputAction.CallbackContext context)
        {
            _lurchInput.UpdateKeyState(context);
        }

        void CheckInput()
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
         Vector3 Accelerate(Vector3 currentVelocity, Vector3 wishDirection, float accelerationRate,
            float accelerationLimit)
        {
            float speed = Vector3.Dot(Vector3.Scale(_plane, currentVelocity),
                Vector3.Scale(wishDirection, _plane).normalized);
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
        Vector3 ApplyFriction(Vector3 currentVelocity, float friction)
        {
            return currentVelocity * (1 / (friction + 1));
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
            /*switch (CurrentCharacterState) {
            case MotorState.Default:
            {
                if (CurrentBonusOrientationMethod == BonusOrientationMethod.TowardsGravity) {
                    // Rotate from current up to invert gravity
                    Vector3 smoothedGravityDir = Vector3.Slerp(Vector3.up, -CurrentGravity.normalized,
                        1 - Mathf.Exp(-BonusOrientationSharpness*deltaTime));
                    currentRotation = Quaternion.FromToRotation(Vector3.up, smoothedGravityDir)*currentRotation;
                } else if (CurrentBonusOrientationMethod == BonusOrientationMethod.TowardsGroundSlopeAndGravity) {
                    if (Motor.GroundingStatus.IsStableOnGround) {
                        Vector3 smoothedGroundNormal = Vector3.Slerp(Vector3.up,
                            Motor.GroundingStatus.GroundNormal,
                            1 - Mathf.Exp(-BonusOrientationSharpness*deltaTime));
                        currentRotation = Quaternion.FromToRotation(Vector3.up, smoothedGroundNormal)*
                            currentRotation;


                        /* Move the position to create a rotation around the bottom hemi center instead of around the pivot
                        Motor.SetTransientPosition(initialCharacterBottomHemiCenter +
                                                   (currentRotation * Vector3.down * Motor.Capsule.radius));
                    } else {
                        Vector3 smoothedGravityDir = Vector3.Slerp(Vector3.up, -CurrentGravity.normalized,
                            1 - Mathf.Exp(-BonusOrientationSharpness*deltaTime));
                        currentRotation = Quaternion.FromToRotation(Vector3.up, smoothedGravityDir)*
                            currentRotation;
                    }
                } else {
                    Vector3 smoothedGravityDir = Vector3.Slerp(Vector3.up, Vector3.up,
                        1 - Mathf.Exp(-BonusOrientationSharpness*deltaTime));
                    currentRotation = Quaternion.FromToRotation(Vector3.up, smoothedGravityDir)*currentRotation;
                }

                break;
            }
            }*/
            currentRotation = Rotation;
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its velocity should be right now. 
        /// This is the ONLY place where you can set the character's velocity
        /// </summary>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
            _previousPosition = CameraPosition.position;
            // Here, we're just going to set the velocity to be the desired velocity we keep in this character motor
            // For two reasons:
            // 1) We want to be able to control the character's velocity in any way we want
            // 2) I am not shoving all of my movement code here. Fuck you.
            currentVelocity = _velocity;

            Vector3 currentUpD = Motor.TransientRotation * Vector3.up;
            DrawVector(Motor.TransientPosition, Vector3.up, 25, Color.red);
            DrawVector(Motor.TransientPosition, -Vector3.up, 25, Color.blue);
        }



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
            DrawVector(Motor.TransientPosition, _velocity, 25, Color.green);
            
            if (
                _mHHit
            ) {
                CalculateOnHitVelocity(_currentNormal);
                _mHHit = false;
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

        public Vector3 Flatten(Vector3 vec)
        {
            return new Vector3(vec.x, 0f, vec.z);
        }

        bool _mHHit = false;
        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
                                  ref HitStabilityReport hitStabilityReport)
        {
            _currentNormal = hitNormal;
            _isStepping = hitStabilityReport.ValidStepDetected;
            _mHHit = true;
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
                                              Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
            // kanye west
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
            
        }

        void OnLanded()
        {
            if (CurrentCharacterState == MotorState.Default)
            {
                CurrentGravity = Gravity;
            }
            // incur a small penalty
            _velocity -= _velocity * 0.23f;
            uint a = 255_5;
            //Debug.Log("Movement penalty incurred");
        }

        void OnLeaveStableGround()
        {
            CanDoubleJump = true;
        }

        void DrawVector(Vector3 origin, Vector3 vector, float lengthMultiplier, Color color)
        {
            Debug.DrawLine(origin, origin + vector * lengthMultiplier, color);
        }

        #endregion
    }
}