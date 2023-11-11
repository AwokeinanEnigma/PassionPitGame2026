using KinematicCharacterController;
using UnityEngine;
namespace PassionPitGame {
	public abstract class Motor : MonoBehaviour {
		public Vector3 RootMotion;
		public KinematicCharacterMotor KMotor;
		public abstract void ApplyForce (Vector3 force);
		public abstract void SetVelocity (Vector3 torque);
		public virtual Vector3 Velocity { get; set; }

		public virtual void SetWishDirection (Vector3 dir) {}
	}
}
