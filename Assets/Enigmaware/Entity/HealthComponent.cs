#region

using UnityEngine;

#endregion
namespace Enigmaware.Entities {

	public class HealthComponent : MonoBehaviour {
		[Tooltip("The maximum health of this object.")]
		public int BaseHealth;

		public bool God;

		[SerializeField]
		[Tooltip("The current health of this object. This value will be reset to 'BaseHealth' when the object is enabled.")]
		int _health;
		public TeamComponent.Team Team;
		IOnDeath[] _onDeathReceivers;

		// used to prevent multiple calls to 'Death' in a single frame
		bool _oneTimeDeath;

		// damage interfaces
		// they're neat
		IOnIncomingDamage[] _onIncomingDamageReceivers;
		IOnTakeDamage[] _onTakeDamageReceivers;

        /// <summary>
        ///     Returns whether or not this object is alive.
        /// </summary>
        public bool Alive {
			get => _health > 0;
		}
		public void Awake () {
			_health = BaseHealth;

			// create arrays of all the damage-related message receivers
			_onTakeDamageReceivers = GetComponents<IOnTakeDamage>();
			_onIncomingDamageReceivers = GetComponents<IOnIncomingDamage>();
			_onDeathReceivers = GetComponents<IOnDeath>();

			Team = GetComponent<TeamComponent>().team;
		}

		public void FixedUpdate () {
			if (!Alive && !_oneTimeDeath) {
				_oneTimeDeath = true;
				Death();
			}
		}

		void Death () {
			Debug.Log($"GameObject '{gameObject.name}' has died.");
			for (int i = 0; i < _onDeathReceivers.Length; i++) {
				_onDeathReceivers[i].OnDeath();
			}
		}

		public void TakeDamage (DamageInfo damageInfo) {
			for (int i = 0; i < _onIncomingDamageReceivers.Length; i++) {
				_onIncomingDamageReceivers[i].OnIncomingDamage(ref damageInfo);
			}

			// if the damage was rejected, don't do anything
			// and if the object is invincible, don't do anything
			if (damageInfo.Rejected || God) {
				return;
			}

			//damage logic here
			_health -= (int)damageInfo.Damage;

			//make new damage report
			var report = new DamageReport {
				Victim = gameObject,
				Attacker = damageInfo.Attacker,
				DamageInfo = damageInfo,
				KillingBlow = _health <= 0
			};

			for (int i = 0; i < _onTakeDamageReceivers.Length; i++) {
				Debug.Log("1");
				_onTakeDamageReceivers[i].OnTakeDamage(report);
			}

			if (damageInfo.Attacker) {
				//deez nuts

				// notify attacker that it has dealt damage
				IOnDamageOther[] iOnDamageOtherArray = damageInfo.Attacker.GetComponents<IOnDamageOther>();
				for (int i = 0; i < iOnDamageOtherArray.Length; i++) {
					iOnDamageOtherArray[i].OnDamageDealt(report);
				}
			}
		}
	}
}
