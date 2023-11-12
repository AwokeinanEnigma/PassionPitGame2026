using Enigmaware.Entities;
using System.Collections.Generic;
using UnityEngine;
namespace PassionPitGame {
	public class PunchAUX : AUXState{
		private MeleeAttack meleeAttack;
		public override void OnEnter () {
			base.OnEnter();
			meleeAttack = new MeleeAttack() {
				 HitboxGroupHandler = base.stateMachine.GetComponentInChildren<HitboxGroupHandler>(),
				Team = TeamComponent.Team.Player,
				DamageInfo = new() {
					Damage = 12,
					Attacker = gameObject,
					Inflictor = gameObject,
					Force = new Vector3(0,10,0)
				}
			};
			
		}
		public override void OnClick () {
			List<MeleeAttack.AttackResult> result = meleeAttack.Hit();
			foreach (MeleeAttack.AttackResult attackResult in result) {
				Motor motor = attackResult.HealthComponent.GetComponent<Motor>();
				motor.KMotor.ForceUnground();
				motor.SetVelocity( Flatten(GetComponentInChildren<Camera>().transform.forward )* 25 + Vector3.up * 12.5f);
				
				//motor.SetVelocity((GetComponentInChildren<Camera>().transform.forward * 35 ) + Vector3.up * 10);
			}

		}
		public Vector3 Flatten (Vector3 f) {
			return new Vector3(f.x, 0, f.z);
		}
	}
}
