#region

using PassionPitGame;
using System.Collections.Generic;
using UnityEngine;

#endregion
namespace Enigmaware.Projectiles {

	public class Projectile : ProjectileBase, IProjectileImpactBehaviour {
		public float speed;
		public void FixedUpdate () {
			body.velocity = projectileController.moveDir*(speed*1000)*Time.fixedDeltaTime;
		}

		public void OnImpact (ProjectileImpactInfo impactInfo) {
			List<ExplosionAttack.Result> results = new ExplosionAttack() {
				Position = impactInfo.pointOfImpact,
				Radius = 10,
				HitEnvironment = false,
				HitEveryone = false,
				Team = team.team,
				LayerMask = LayerMask.GetMask("Hurtbox"),
				MaximumHits = 25,
				UseAccuracy = false,
				Visualize = true,
				
			}.Fire();
			Debug.Log(results.Count);
			results.ForEach(result => {
				CharacterMotor motor = result.HealthComponent.GetComponent<CharacterMotor>();
				motor.Motor.ForceUnground();
				//Debug.Log(result.HitPoint - transform.position);
				motor.SetVelocity(Flatten(result.HitPoint - transform.position).normalized * 13 + Vector3.up * 15);
			});
			Destroy(gameObject);
		}
		
		Vector3 Flatten (Vector3 f) {
			return new Vector3(f.x, 0, f.z);
		}
	}
}
