#region

using UnityEngine;

#endregion

namespace PassionPitGame {
    public class CameraController : MonoBehaviour {

        public const float VIEWBOBBING_SPEED = 8;
        public float LookScale = 1;

        public Camera PlayerCamera;

        public CharacterMotor CharacterMotor;

        #region Properties

        [HideInInspector]
        public Vector3 Up {
            get => PlayerCamera.transform.up;
            set => PlayerCamera.transform.up = value;
        }

        [HideInInspector]
        public Vector3 Position {
            get => PlayerCamera.transform.position;
            set => PlayerCamera.transform.position = value;
        }

        [HideInInspector]
        public Vector3 PlanarForward => Vector3.Scale(_plane, PlayerCamera.transform.forward).normalized;

        [HideInInspector] public Vector3 PlanarRight => -Vector3.Cross(PlanarForward, Vector3.up).normalized;

        [HideInInspector]
        public Vector3 Forward {
            get => PlayerCamera.transform.forward;
            set => PlayerCamera.transform.forward = value;
        }

        [HideInInspector]
        public Vector3 Right {
            get => PlayerCamera.transform.right;
            set => PlayerCamera.transform.right = value;
        }

        [HideInInspector]
        public Quaternion Rotation {
            get => PlayerCamera.transform.rotation;
            set => PlayerCamera.transform.rotation = value;
        }

        public Vector3 PlanarDirection { get; set; }
        readonly Vector3 _plane = new Vector3(1, 0, 1);
        #endregion

        
        private float cameraRotation;
        private float cameraRotationSpeed;
        private float previousYVelocity;
        private float threeSixtyCounter;

        private float velocityThunk;
        private float velocityThunkSmoothed;
        private float viewBobbingAmount;
        public float Yaw { get; set; }
        public float YawFutureInterpolation { get; set; }
        public float YawIncrease { get; set; }
        public float Pitch { get; set; }
        public float PitchFutureInterpolation { get; set; }
        public float CameraRoll { get; set; }

        private void Awake () {
            Cursor.lockState = CursorLockMode.Locked;
            PlanarDirection = Vector3.forward;
        }
        public void Update () {
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

            // This is where orientation is handled, the PlayerCamera is only adjusted by the pitch, and the entire player is adjusted by yaw
            velocityThunk = Mathf.Lerp(velocityThunk, 0, Time.deltaTime*4);
            velocityThunkSmoothed = Mathf.Lerp(velocityThunkSmoothed, velocityThunk, Time.deltaTime*16);
             
            velocityThunk += (CharacterMotor.Velocity.y - previousYVelocity)/3f;
            previousYVelocity = CharacterMotor.Velocity.y;

            viewBobbingAmount -= Mathf.Min(Time.deltaTime*3, viewBobbingAmount);
            var yawBobbing = (Mathf.Sin((Time.time*VIEWBOBBING_SPEED) + Mathf.PI/2) - 0.5f)*viewBobbingAmount*0.6f;
            var pitchBobbing = (Mathf.Abs(Mathf.Sin(Time.time*VIEWBOBBING_SPEED)) - 0.5f)*viewBobbingAmount*0.4f;

            PlayerCamera.transform.localRotation =
                Quaternion.Euler(new Vector3(Pitch + velocityThunkSmoothed - pitchBobbing, 0, CameraRoll));
            CharacterMotor.Rotation = Quaternion.Euler(0, Yaw + yawBobbing, 0);

            // This value is used to calcuate the positions in between each fixedupdate tick

            CameraRoll -= Mathf.Sign(CameraRoll - cameraRotation)*Mathf.Min(cameraRotationSpeed*Time.deltaTime,
                Mathf.Abs(CameraRoll - cameraRotation));
            PlayerCamera.transform.position = CharacterMotor.InterpolatedPosition;
        }
    }
}
