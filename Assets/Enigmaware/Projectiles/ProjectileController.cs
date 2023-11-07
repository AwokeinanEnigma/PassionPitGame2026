#region

using System;
using UnityEngine;

#endregion
namespace Enigmaware.Projectiles {

	public struct ProjectileImpactInfo {
		public Collider collidingCollider;
		public Vector3 pointOfImpact;
		public Vector3 impactNormal;
	}

	public interface IProjectileImpactBehaviour {
		void OnImpact (ProjectileImpactInfo impactInfo);
	}
	public struct ProjectileInfo {
		// / rotation & position /
		public Quaternion rotation;
		public Vector3 position;
		// / projectile /
		public GameObject projectilePrefab;
		// / owner /
		public GameObject owner;

		public DamageInfo damageInfo;

		public Vector3 force;
		public Vector3 moveDir;

		public TeamComponent.Team team;

		public GameObject target;

		public event Action<GameObject> onProjectileCreated;

		public void OnProjectileSetup (GameObject live) {
			onProjectileCreated?.Invoke(live);
		}
	}
	public class ProjectileController : MonoBehaviour {
		public GameObject ownerObject;

		public Vector3 moveDir;
		Collider[] currentColliders;

		Rigidbody rigidbody;
		public Transform ownerTransform {
			get {
				if (ownerObject) {
					return ownerObject.transform;
				}
				Debug.LogError("Projectile: " + gameObject.name + " doesn't have an owner!");
				return null;
			}
		}

		/*public void OnTriggerStay(Collider collision)
		{
		    Destroy(this.gameObject);
		}*/

		void Awake () {
			rigidbody = GetComponent<Rigidbody>();
			currentColliders = GetComponents<Collider>();
			for (int i = 0; i < currentColliders.Length; i++) {
				currentColliders[i].enabled = false;
			}
		}
		void Start () {
			for (int i = 0; i < currentColliders.Length; i++) {
				currentColliders[i].enabled = true;
			}
			IgnoreCollisionWithOwner();
		}


		// Update is called once per frame
		public void OnCollisionEnter (Collision collision) {
			ContactPoint[] contacts = collision.contacts;
			ProjectileImpactInfo impactInfo = new ProjectileImpactInfo {
				collidingCollider = collision.collider,
				pointOfImpact = EstimateContactPoint(contacts, collision.collider),
				impactNormal = EstimateContactNormal(contacts)
			};
			//Debug.Log("Projectile: " + base.gameObject.name + " has collided with: " + collision.gameObject.name);
			foreach (IProjectileImpactBehaviour impact in GetComponents<IProjectileImpactBehaviour>()) {
				impact.OnImpact(impactInfo);
			}
		}
		// Use this for initialization

		public static void LaunchProjectile (ProjectileInfo info) {
			GameObject projectile = Instantiate(info.projectilePrefab, info.position, info.rotation);
			InitProjectile(projectile, info);

		}

		static void InitProjectile (GameObject projectile, ProjectileInfo info) {
			//offload setting references somewhere else   

			ProjectileController controller = projectile.GetComponent<ProjectileController>();
			controller.ownerObject = info.owner;
			controller.moveDir = info.moveDir;

			ProjectileDamageContainer damageInfo = projectile.GetComponent<ProjectileDamageContainer>();
			if (damageInfo != null) {
				damageInfo.DamageInfo = info.damageInfo;
			}

			ProjectileTargetHolder tHolder = projectile.GetComponent<ProjectileTargetHolder>();
			if (tHolder != null) {
				tHolder.target = info.target;
			}

			TeamComponent team = projectile.GetComponent<TeamComponent>();
			if (team != null) {
				team.team = info.team;
			}

			info.OnProjectileSetup(projectile);

		}

		//Ignore colliding with our owner.
		void IgnoreCollisionWithOwner () {
			if (ownerObject) {
				foreach (Collider ownerCollider in ownerObject.GetComponentsInChildren<Collider>()) {
					for (int i = 0; i < currentColliders.Length; i++) {
						Collider myCollider = currentColliders[i];
						Physics.IgnoreCollision(ownerCollider, myCollider);

					}
				}
			} else {
				Debug.LogWarning("Projectile: " + gameObject.name + " doesn't have an owner! Colliding with everything!!");
			}
		}

		#region Collider estimation functions.

		Vector3 EstimateContactPoint (ContactPoint[] contacts, Collider collider) {
			if (contacts.Length == 0) {
				return collider.transform.position;
			}
			return contacts[0].point;
		}

		Vector3 EstimateContactNormal (ContactPoint[] contacts) {
			if (contacts.Length == 0) {
				return Vector2.zero;
			}
			return contacts[0].normal;
		}

		#endregion
	}
}
