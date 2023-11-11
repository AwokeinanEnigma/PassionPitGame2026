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
			//OriginalGravity = Motor.Gravity;
			//Motor.Gravity = OriginalGravity/ 1.5f;
			Projectile = Resources.Load<GameObject>("Prefabs/LiterallyMe 1");
 			ProjectileSpawnPoint = GetComponent<TransformDictionary>().FindTransform("ProjectilePoint");
            
            Motor.KMotor.ForceUnground();
            Vector3 velocity = Motor.KMotor.Velocity;
            velocity.y = 13;
            Motor.Velocity = velocity;
		}
		
		public override void OnExit () {
			base.OnExit();
		}
		
		
		public override void OnClick () {
			/*ProjectileInfo info = new ProjectileInfo()
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
			ProjectileController.LaunchProjectile(info);*/
			for (int i = 0; i < 8; i++)
			{
				float angle = i * Mathf.PI * 2f / 8;
				var position1 = transform.position;
				Vector3 newPos = new Vector3(position1.x + Mathf.Cos(angle) * 5, position1.y, position1.z + Mathf.Sin(angle) * 5);
				
				Vector3 direction = newPos - position1 + (-Vector3.up * 6);
				Vector3 normalizedDirection = direction.normalized;
				Vector3 force = normalizedDirection;
				
				ProjectileInfo info = new ProjectileInfo()
				{
					projectilePrefab = Projectile,
					owner = this.gameObject,
					moveDir = force,
					position = newPos,
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
				
				
				//Instantiate(prefab, newPos, );
			}


			OnExit();
		}
		
		public override bool CanBeInterrupted () {
			return true;
		}
	}
}
