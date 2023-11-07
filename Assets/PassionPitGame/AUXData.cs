using UnityEngine;
namespace PassionPitGame {
	[CreateAssetMenu(fileName = "AUX Card", menuName = "AUX", order = 0)]
	public class AUXData : ScriptableObject {
		public SerializableEntityStateType StateType;
		public string Name;
		public string Description;
		public GameObject CardPrefab;
		public int Cost;
		public bool IsImportant;
	}
}
