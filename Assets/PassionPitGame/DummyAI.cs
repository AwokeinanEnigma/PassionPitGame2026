using Enigmaware.AI;
using EntityStates;
using UnityEngine;
namespace PassionPitGame {
	public class DummyAI : EntityState
	{
		public AdvancedAIMover mover;
		Transform PlayerTransform;
		public override void OnEnter () {
			base.OnEnter();
			PlayerTransform = GameObject.Find("PlayerObject").transform;
			mover = GetComponent<AdvancedAIMover>();
		}

		public override void FixedUpdate () {
			base.FixedUpdate();
			mover.destination = PlayerTransform.position;
		}
		
	}
}
