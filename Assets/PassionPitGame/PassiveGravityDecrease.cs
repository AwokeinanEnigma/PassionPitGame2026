using UnityEngine;
namespace PassionPitGame {
	public class PassiveGravityDecrease : AUXState {

		public Vector3 OriginalGravity;
		public CharacterMotor Motor;
		public override void OnEnter () {
			base.OnEnter();
			Motor = base.stateMachine.GetComponent<CharacterMotor>();
			OriginalGravity = Motor.Gravity;
			Motor.Gravity = OriginalGravity/ 2;
		}
		
		public override void OnExit () {
			base.OnExit();
			Motor.Gravity = OriginalGravity;
		}
		
		
		public override void OnClick () {
			OnExit();
		}
		
		public override bool CanBeInterrupted () {
			return true;
		}
	}
}
