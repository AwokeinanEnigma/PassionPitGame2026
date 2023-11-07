using UnityEngine;
using KinematicCharacterController;


namespace Enigmaware.Gravity
{
    public class PlanetRotator : MonoBehaviour, IMoverController
    {
        public PhysicsMover PlanetMover;
        public float GravityStrength = 10;
        public Vector3 OrbitAxis = Vector3.forward;
        public float OrbitSpeed = 10;
        private Quaternion _lastRotation;

        private void Start()
        {
            _lastRotation = PlanetMover.transform.rotation;

            PlanetMover.MoverController = this;
        }

        public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            goalPosition = PlanetMover.Rigidbody.position; // + transform.forward * Time.deltaTime;


            // Rotate
            Quaternion targetRotation = Quaternion.Euler(OrbitAxis * OrbitSpeed * deltaTime) * _lastRotation;
            goalRotation = targetRotation;
            _lastRotation = targetRotation;
        }
    }
}