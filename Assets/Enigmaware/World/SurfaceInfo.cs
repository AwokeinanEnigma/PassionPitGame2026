#region

using UnityEngine;

#endregion
namespace Enigmaware.World {
    /// <summary>
    ///     Holds information about a surface.
    /// </summary>
    [CreateAssetMenu(fileName = "SurfaceInfo", menuName = "Enigmaware/Movement/SurfaceInfo", order = 0)]
	public class SurfaceInfo : ScriptableObject {
		public enum WallrunRefreshType {
			// This is for cubes.
			// Basically we can wall run if we hit another side of the same collider 
			OKifDIFFERENTSIDE,
			// This is for curves.
			// Basically we can wall run if the same wall collider timer is finished
			OKifDURATIONPASS
		}

		public bool IsWallrunable = true;
		public bool IsGrappable = true;
		public bool IsMoving;
		public float Drag = 1;

		public WallrunRefreshType WallrunRefresh = WallrunRefreshType.OKifDIFFERENTSIDE;

		public static SurfaceInfo FindSurfaceInfo (Collider collider) {
			SurfaceHolder holder = collider.GetComponent<SurfaceHolder>();
			if (holder) {
				return holder.SurfaceInfo;
			}
			return null;
		}
	}
}
