using System;
using UnityEngine;
namespace PassionPitGame {
	public class GaylordAnimator : MonoBehaviour {
		public Sprite Idle;
		public Sprite Air;
		
		public SpriteRenderer Renderer;
		public AIMotor Motor;

		public void Update () {
			Renderer.sprite = Motor.IsGrounded ? Idle : Air;
		}
	}
}
