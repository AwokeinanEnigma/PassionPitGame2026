#region

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

#endregion

//Attach this script to a GameObject to rotate around the target position.
public class TransformDictionary : MonoBehaviour {

	[FormerlySerializedAs("pairings")]
	public List<ChildNamePair> Pairings;

	public Transform FindTransform (string name) {
		for (int i = 0; i < Pairings.Count; i++) {
			ChildNamePair pair = Pairings[i];
			if (pair.Name == name) {
				return pair.Transform;
			}
		}

		return null;
	}
	[Serializable]
	public struct ChildNamePair {
		[FormerlySerializedAs("name")]
		public string Name;
		[FormerlySerializedAs("transform")]
		public Transform Transform;
	}
}
