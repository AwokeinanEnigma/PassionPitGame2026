#region

using UnityEngine;

#endregion
namespace Enigmaware.Projectiles {
	public class ProjectileBase : MonoBehaviour {
		public ProjectileController projectileController;
		public Rigidbody body;
		public GameObject owner;
		public ProjectileDamageContainer damageContainer;
		public ProjectileTargetHolder targetHolder;

		public GameObject target;
		public virtual void Awake () {
			projectileController = GetComponent<ProjectileController>();
			body = GetComponent<Rigidbody>();
			damageContainer = GetComponent<ProjectileDamageContainer>();
			targetHolder = GetComponent<ProjectileTargetHolder>();
			// Debug.Log("Hey!");
		}

		public virtual void Start () {
			if (projectileController && projectileController.ownerObject) {
				owner = projectileController.ownerObject;
			}

			if (targetHolder) {
				target = targetHolder.target;
			}
		}
	}
}
