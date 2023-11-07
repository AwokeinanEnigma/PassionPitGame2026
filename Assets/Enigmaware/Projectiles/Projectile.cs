#region

using UnityEngine;

#endregion
namespace Enigmaware.Projectiles {

	public class Projectile : ProjectileBase, IProjectileImpactBehaviour {
		public float speed;
		public void FixedUpdate () {
			body.velocity = projectileController.moveDir*(speed*1000)*Time.fixedDeltaTime;
		}

		public void OnImpact (ProjectileImpactInfo impactInfo) {
			Destroy(gameObject);
		}
	}
}
