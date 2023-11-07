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
	}
}
