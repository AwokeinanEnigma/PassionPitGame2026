using UnityEngine;
namespace PassionPitGame {
	public class DestroyOnDeath : MonoBehaviour, IOnDeath {

		public void OnDeath () {
			Destroy(gameObject);
		}
	}
}
