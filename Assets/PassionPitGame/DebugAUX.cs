using UnityEngine;
namespace PassionPitGame {
	public class DebugAUX : AUXState{

		public override void OnClick () {
			OnExit();
		}
		
		public override bool CanBeInterrupted () {
			return true;
		}
	}
}
