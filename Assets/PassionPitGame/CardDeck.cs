using EntityStates;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace PassionPitGame {
	public class CardDeck : MonoBehaviour {
		public AUXData[] Cards;
		public EntityStateMachine Machine;
		public int CurrentCardIndex;
		InputEventData _inputEventData = new();
		AUXState _currentState;
		public void Awake () {
			Machine = GetComponent<EntityStateMachine>();
			Machine.OnStateSwitch += OnStateSwitch;
			
		}

		void Start () {
			Machine.SetState(EntityState.InstantiateState(Cards[CurrentCardIndex].StateType.type));
		}

		public void OnStateSwitch () {
			Debug.Log((Machine.currentState));
			_currentState = Machine.currentState as AUXState;
		}

		public void Click (InputAction.CallbackContext callbackContext) {
			_inputEventData.UpdateKeyState(callbackContext);
		}

		public void Update () {
			if (_inputEventData.down) {
				_currentState.OnClick();
				Debug.Log("clic!");
				CurrentCardIndex++;
				if (CurrentCardIndex >= Cards.Length) {
					CurrentCardIndex = 0;
				}
				Machine.SetState(EntityState.InstantiateState(Cards[CurrentCardIndex].StateType.type));
			}
		}
		
		public void OnGUI () {
			GUI.Label(new Rect(0, 0, 100, 100), Cards[CurrentCardIndex].Name);
		}

		public void LateUpdate () {
			_inputEventData.RefreshKeyData();
		}
	}
}
