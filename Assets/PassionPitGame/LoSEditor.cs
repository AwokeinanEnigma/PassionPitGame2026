using PassionPitGame;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LoS))]
public class LoSEditor : Editor
{
	private void OnSceneGUI()
	{
		LoS fov = (LoS)target;
		Handles.color = Color.white;
		var position = fov.transform.position;
		Handles.DrawWireArc(position, Vector3.up, Vector3.forward, 360, fov.radius);

		Vector3 viewAngle01 = DirectionFromAngle(fov.transform.eulerAngles.y, -fov.angle / 2);
		Vector3 viewAngle02 = DirectionFromAngle(fov.transform.eulerAngles.y, fov.angle / 2);

		Handles.color = Color.yellow;
		Handles.DrawLine(position, position + viewAngle01 * fov.radius);
		Handles.DrawLine(position, position + viewAngle02 * fov.radius);

		if (fov.canSeePlayer)
		{
			Handles.color = Color.green;
			Handles.DrawLine(fov.transform.position, fov.playerRef.transform.position);
		}
		if (fov.target) {
			Handles.DrawLine(fov.transform.position, fov.target.transform.position);
		}
	}

	private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
	{
		angleInDegrees += eulerY;

		return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
	}
}
