#region

using UnityEngine;

#endregion
namespace Enigmaware.Entities {
	public class Hurtbox : MonoBehaviour {
		public enum HurtboxType {
			Head,
			Body,
			WeakPoint,
			Package,
		}

		public Collider Collider;
		public HurtboxType type = HurtboxType.Body;
		public HealthComponent healthComponent;
		public void Awake () {
			if (!Collider) Collider = GetComponent<Collider>();
		}
	}
}
