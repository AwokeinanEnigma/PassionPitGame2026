using Enigmaware.Projectiles;
using UnityEngine;
namespace PassionPitGame {
	public class DashAUX : AUXState {
		public CharacterMotor Motor;
		public override void OnEnter () {
			base.OnEnter();
			Motor = GetComponent<CharacterMotor>();
		}

		bool dash;
		Vector3 DashVector;
		Vector3 BeforeDashVector;
		public const float DASH_SPEEDGAIN = 14;
		public const float DASH_SPEEDGAIN_CAP = 7f;
		Vector3 _footPosition;
		public override void OnClick () {
			BeforeDashVector = Motor.Velocity;
			dash = true;
			Motor.ForceMovementType(CharacterMotor.MovementType.Deferred);
			if (Motor.WishDirection.magnitude > 0) {
				DashVector = Motor.WishDirection;
			} else {
				DashVector = Motor.transform.forward;
			}
			Vector3 position = transform.position;
			position.y -= Motor.KMotor.Capsule.height * 0.5f;
			_footPosition = position;
			ProjectileInfo info = new ProjectileInfo()
			{
				projectilePrefab = _projectile,
				owner = this.gameObject,
				moveDir = Vector3.down * 2,
				position = _footPosition,
				team = TeamComponent.Team.Player,
				damageInfo = new DamageInfo()
				{
					Attacker = base.gameObject,
					Damage = 1,
					Force = Vector3.up * 20,
					Inflictor = gameObject,
				}
			};
			//ProjectileController.LaunchProjectile(info);
		}
		float _stopwatch;
		float _pStopwatch;
		float _interval = 0.01f;
		GameObject _projectile = Resources.Load<GameObject>("Prefabs/LiterallyMe 1");
		public override void FixedUpdate () {
			base.FixedUpdate();
			
			if (dash) { 
				_stopwatch += Time.fixedDeltaTime;
				_pStopwatch += Time.fixedDeltaTime;
				Motor.Velocity = Vector3.zero;
				Motor.RootMotion += DashVector * (7 * 14 * Time.fixedDeltaTime);
				// this should only spawn 7 projectiles
				// ( 0.07 / 0.01 )
				if (_stopwatch > 0.07F) {
					var wishdir = Motor.RootMotion.normalized;

					var y = Motor.CalculateYForDirectionAndSpeed(wishdir, Motor.Flatten(BeforeDashVector).magnitude, 30);

					var onlyYChange = Motor.Flatten(BeforeDashVector).magnitude * Motor.Flatten(wishdir).normalized;
					onlyYChange.y = y;

					Motor.Velocity = onlyYChange.magnitude * wishdir.normalized;

					var rawgain = DASH_SPEEDGAIN *
						Mathf.Clamp01((DASH_SPEEDGAIN_CAP - Motor.Speed) / DASH_SPEEDGAIN_CAP);
					Motor.Velocity += Motor.Flatten(Motor.Velocity).normalized * rawgain;

					Motor.CanDoubleJump = true;
					ProjectileInfo info = new ProjectileInfo()
					{
						projectilePrefab = _projectile,
						owner = this.gameObject,
						moveDir = Vector3.down * 2,
						position = _footPosition,
						team = TeamComponent.Team.Player,
						damageInfo = new DamageInfo()
						{
							Attacker = base.gameObject,
							Damage = 1,
							Force = Vector3.up * 20,
							Inflictor = gameObject,
						}
					};
					//ProjectileController.LaunchProjectile(info);
					dash = false; ;
					Motor.ForceMovementType(CharacterMotor.MovementType.Air);
					CardDeck.ForceSwitch();
				}
			}
		}
		
		public override void OnExit () {
			base.OnExit();
			Motor.ForceMovementType(CharacterMotor.MovementType.Air);
		}
		
		public override bool CanBeInterrupted () {
			return false;
		}
	}
}
