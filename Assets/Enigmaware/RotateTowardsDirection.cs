using UnityEngine;

namespace Enigmaware.General
{
	public class RotateTowardsDirection : MonoBehaviour {
        
		public float Speed;
		public float MaxDegrees;
		public Vector3 Direction;

		public void Update()
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation,SimulateRotationTowards(Direction, MaxDegrees), Speed * Time.deltaTime); 
		}

		protected Quaternion SimulateRotationTowards (Vector3 direction, float maxDegrees) {
			if (direction != Vector3.zero) {
				Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
				// This causes the character to only rotate around the Z axis
				//targetRotation *= Quaternion.Euler(90, 0, 0);
				return Quaternion.RotateTowards(transform.rotation, targetRotation, maxDegrees);
			}
			return Quaternion.identity;
		}
	}
}
