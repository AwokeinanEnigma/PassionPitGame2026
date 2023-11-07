#region

using UnityEngine;

#endregion
public class TeamComponent : MonoBehaviour {
	public enum Team {
		Player,
		Enemy,
	}
	public Team team;

	public static bool FliterAttack (TeamComponent src, TeamComponent dest) {
		Debug.Log($"And what do I my eyes glaze upon? src='{src.gameObject}' dest='{dest.gameObject}'");
		if (src.team != dest.team) {
			return true;
		}
		return false;
	}
}
