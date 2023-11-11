#region

using Enigmaware.Motor;
using Enigmaware.Projectiles;
using PassionPitGame;
using System.Collections.Generic;
using UnityEngine;

#endregion
namespace PassionPitGame {

	public class PlayerProjectile : ProjectileBase, IProjectileImpactBehaviour {
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
			results.ForEach(result => {
				Motor motor = result.HealthComponent.GetComponent<Motor>();
				motor.KMotor.ForceUnground();
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
