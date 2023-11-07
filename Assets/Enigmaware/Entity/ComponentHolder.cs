#region

using System;
using UnityEngine;

#endregion
namespace Enigmaware.General {
	public class ComponentHolder : MonoBehaviour {

		public ComponentNameAssociation[] Components;

		public MonoBehaviour Find (string name) {
			foreach (ComponentNameAssociation component in Components) {
				if (component.name == name) {
					return component.component;
				}
			}

			throw new Exception($"Component {name} not found!");
		}
		[Serializable]
		public struct ComponentNameAssociation {
			public MonoBehaviour component;
			public string name;

			public ComponentNameAssociation (MonoBehaviour component, string name) {
				this.component = component;
				this.name = name;
			}
		}
	}
}
