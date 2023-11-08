using UnityEngine;
namespace PassionPitGame {
	public class BoostAUX : AUXState{

		public override void OnClick () {
			//Debug.Log(Input.GetKeyDown("Jump"));
			//base.stateMachine.GetComponent<CharacterMotor>().Motor.ForceUnground();
			CharacterMotor motor = GetComponent<CharacterMotor>();
			motor.Motor.ForceUnground();
			Vector3 velocity = motor.Motor.Velocity;
			velocity.y = 25;
			motor.Velocity = velocity;
			//base.stateMachine.GetComponent<CharacterMotor>().Velocity += (new Vector3(0,25,0));
			OnExit();
		}
		
		public override bool CanBeInterrupted () {
			return true;
		}
	}
}
