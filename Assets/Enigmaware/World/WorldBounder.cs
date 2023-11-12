#region

using UnityEngine;

#endregion
namespace Enigmaware.World {
    /// <summary>
    ///     Registers a GameObject to the World Bound Register, which destroys the object when it goes out of bounds.
    /// </summary>
    public class WorldBounder : MonoBehaviour {
		public void Start () {
			WorldBoundRegister.instance.RegisterObject(gameObject);
		}
		public static void DebugBounds(Bounds bounds, UnityEngine.Color color, float duration)
		{
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;
			Vector3 start = new Vector3(min.x, min.y, min.z);
			Vector3 vector = new Vector3(min.x, min.y, max.z);
			Vector3 vector2 = new Vector3(min.x, max.y, min.z);
			Vector3 end = new Vector3(min.x, max.y, max.z);
			Vector3 vector3 = new Vector3(max.x, min.y, min.z);
			Vector3 vector4 = new Vector3(max.x, min.y, max.z);
			Vector3 end2 = new Vector3(max.x, max.y, min.z);
			Vector3 start2 = new Vector3(max.x, max.y, max.z);
			Debug.DrawLine(start, vector, color, duration);
			Debug.DrawLine(start, vector3, color, duration);
			Debug.DrawLine(start, vector2, color, duration);
			Debug.DrawLine(vector2, end, color, duration);
			Debug.DrawLine(vector2, end2, color, duration);
			Debug.DrawLine(start2, end, color, duration);
			Debug.DrawLine(start2, end2, color, duration);
			Debug.DrawLine(start2, vector4, color, duration);
			Debug.DrawLine(vector4, vector3, color, duration);
			Debug.DrawLine(vector4, vector, color, duration);
			Debug.DrawLine(vector, end, color, duration);
			Debug.DrawLine(vector3, end2, color, duration);
		}
	}
}
