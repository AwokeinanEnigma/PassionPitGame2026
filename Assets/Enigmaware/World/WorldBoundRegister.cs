#region

using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

#endregion
namespace Enigmaware.World {

    /// <summary>
    ///     Destroys objects that go out of bounds, and gets those bounds from the A* graph.
    /// </summary>
    public class WorldBoundRegister : MonoBehaviour {
		public static WorldBoundRegister instance;
		public List<GameObject> enemies;
		public Bounds bounds;

		public float PruneInterval = 2;
		float _timer;

		public void Awake () {
			instance = this;
			enemies = new List<GameObject>();
			bounds = new Bounds(AstarData.active.data.recastGraph.forcedBoundsCenter, AstarData.active.data.recastGraph.forcedBoundsSize);
		}

		public void Update () {
			_timer += Time.deltaTime;

			if (_timer >= PruneInterval) {
				_timer = 0;
				Prune();
			}

		}
		public void RegisterObject (GameObject obj) {
			if (!enemies.Contains(obj)) {
				enemies.Add(obj);
			}
		}

		public void UnregisterObject (GameObject obj) {
			if (enemies.Contains(obj)) {
				enemies.Remove(obj);
			}
		}

		public void Prune () {
			for (int i = 0; i < enemies.Count; i++) {
				GameObject obj = enemies[i];
				if (obj == null || !bounds.Contains(obj.transform.position)) {
					Destroy(obj);
					enemies.RemoveAt(i);
				}
			}
		}
	}
}
