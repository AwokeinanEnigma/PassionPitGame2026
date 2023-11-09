using Enigmaware.Projectiles;
using UnityEngine;
namespace PassionPitGame {
	public class PassiveGravityDecrease : AUXState {

		public Vector3 OriginalGravity;
		public CharacterMotor Motor;
		public GameObject Projectile;
		public Transform ProjectileSpawnPoint;
		public override void OnEnter () {
			base.OnEnter();
			Motor = base.stateMachine.GetComponent<CharacterMotor>();
			OriginalGravity = Motor.Gravity;
			Motor.Gravity = OriginalGravity/ 1.5f;
			Projectile = Resources.Load<GameObject>("Prefabs/LiterallyMe 1");
			ProjectileSpawnPoint = GetComponent<TransformDictionary>().FindTransform("ProjectilePoint");
		}
		
		public override void OnExit () {
			base.OnExit();
			Motor.Gravity = OriginalGravity;
		}
		
		
		public override void OnClick () {
			var position = ProjectileSpawnPoint.position;
			ProjectileInfo info = new ProjectileInfo()
			{
				projectilePrefab = Projectile,
				owner = this.gameObject,
				moveDir = Motor.GetComponentInChildren<Camera>().transform.forward,
				position = position,
				team = TeamComponent.Team.Player,
				damageInfo = new DamageInfo()
				{
					Attacker = base.gameObject,
					Damage = 1,
					Force = Vector3.up * 20,
					Inflictor = null,
				}
			};
			ProjectileController.LaunchProjectile(info);

			OnExit();
		}
		
		public override bool CanBeInterrupted () {
			return true;
		}
	}
}
