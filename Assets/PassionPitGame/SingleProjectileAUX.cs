using Enigmaware.Projectiles;
using UnityEngine;
namespace PassionPitGame {
	public class SingleProjectileAUX : AUXState {

		public Vector3 OriginalGravity;
		public CharacterMotor Motor;
		public GameObject Projectile;
		public Transform ProjectileSpawnPoint;
		public override void OnEnter () {
			base.OnEnter();
			Motor = base.stateMachine.GetComponent<CharacterMotor>();
			//OriginalGravity = Motor.Gravity;
			//Motor.Gravity = OriginalGravity/ 1.5f;
			Projectile = Resources.Load<GameObject>("Prefabs/LiterallyMe 1");
			ProjectileSpawnPoint = GetComponent<TransformDictionary>().FindTransform("ProjectilePoint");
		}
		
		public override void OnExit () {
			base.OnExit();
		}
		
		
		public override void OnClick () {
			ProjectileInfo info = new ProjectileInfo()
			{
				projectilePrefab = Projectile,
				owner = this.gameObject,
				moveDir = Motor.GetComponentInChildren<Camera>().transform.forward,
				position = ProjectileSpawnPoint.transform.position,
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
