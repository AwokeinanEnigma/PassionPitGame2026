using UnityEngine;
namespace PassionPitGame {
	public class DestructionTimer : MonoBehaviour {
		public float time;
		public void Start () {
			Destroy(gameObject, time);
		}
	}
}
