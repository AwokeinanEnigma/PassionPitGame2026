#region

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

#endregion

namespace PassionPitGame {
    public class CameraController : MonoBehaviour {
        [HideInInspector] public Vector3 EulerRotation;
        [HideInInspector] public Vector3 FollowEulerRotation;


        public Camera PlayerCamera;



        #region Rotation

        [Range(-90f, 90f)]
        public float DefaultVerticalAngle = 20f;

        [Range(-90f, 90f)]
        public float MinVerticalAngle = -90f;

        [Range(-90f, 90f)]
        public float MaxVerticalAngle = 90f;

        #endregion


        public Vector3 RotationOffset;
        public Vector3 TargetRotationOffset;
        public float MouseSensitivity;

        public float FOVChangeSmoothness;


        public Transform FollowTransform;


        public float CameraTiltSmoothness;
        public float TargetCameraTilt;
        [SerializeField]
        bool _dynamicCamera = true;
        [SerializeField]
        bool _dynamicFOV = true;
        [SerializeField]
        float _targetFOV = 110;

        private Vector2 _input;
        private readonly Vector3 _plane = new(1, 0, 1);

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

        #endregion

        public CharacterMotor Motor;
        
        private void Awake () {
            Cursor.lockState = CursorLockMode.Locked;
            PlanarDirection = Vector3.forward;
        }
        public void Update () {
            base.transform.position = Motor.InterpolatedPosition;
            EulerRotation += new Vector3(-_input.y, 0, 0)*MouseSensitivity;
            PlayerCamera.transform.eulerAngles += new Vector3(0, _input.x, 0)*MouseSensitivity;
            EulerRotation.x = Mathf.Clamp(EulerRotation.x, MinVerticalAngle, MaxVerticalAngle);
            if (!Mathf.Approximately(_targetFOV, PlayerCamera.fieldOfView)) {
                PlayerCamera.fieldOfView = Mathf.Lerp(PlayerCamera.fieldOfView, _targetFOV, FOVChangeSmoothness*Time.deltaTime);
                PlayerCamera.fieldOfView = Mathf.Round(PlayerCamera.fieldOfView*100)/100;
            }
            if (!Mathf.Approximately(EulerRotation.z, TargetCameraTilt)) {
                EulerRotation.z = Mathf.Lerp(EulerRotation.z, TargetCameraTilt, CameraTiltSmoothness*Time.deltaTime);
            }
            if (!Mathf.Approximately(TargetRotationOffset.magnitude, RotationOffset.magnitude)) {
                RotationOffset = Vector3.Lerp(RotationOffset, TargetRotationOffset, CameraTiltSmoothness*Time.deltaTime);
            }
            base.transform.eulerAngles = new Vector3(EulerRotation.x, base.transform.eulerAngles.y, EulerRotation.z) + RotationOffset;

            // Many times I've almost erased this comment
            // Let it be known, that this comment is eternally immortalized in the code
            // You will never be able to erase this comment, not as long as I live, fucker.
            // Finuyuiiiiiiiiiiiiiiiiiiiiiiiiiuuuuuuuuuuukoi8uuuuuuuuuu u uuu uu u u u u u u u u u u u u ud the smoothed camera orbit position
        }
        private void OnValidate () {
            DefaultVerticalAngle = Mathf.Clamp(DefaultVerticalAngle, MinVerticalAngle, MaxVerticalAngle);
        }
        public void Tilt (float angle) {
            if (!_dynamicCamera) return;
            TargetCameraTilt = angle;
        }

        public void SetFOV (float newFOV) {
            if (!_dynamicFOV) return;
            _targetFOV = newFOV;
        }
        
        public void Look (InputAction.CallbackContext context) {
            _input = context.ReadValue<Vector2>();
        }

    }
}
