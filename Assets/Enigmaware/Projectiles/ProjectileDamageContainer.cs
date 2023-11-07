#region

using UnityEngine;

#endregion
namespace Enigmaware.Projectiles {

	public class ProjectileDamageContainer : MonoBehaviour {
		public DamageInfo DamageInfo;
		public void Start () {
			DamageInfo.Inflictor = gameObject;
		}
	}
}
