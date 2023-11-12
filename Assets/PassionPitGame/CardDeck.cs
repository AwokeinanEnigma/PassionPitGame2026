using DG.Tweening;
using EntityStates;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace PassionPitGame {
	public class CardDeck : MonoBehaviour {
		[Header("UI")]
		public GameObject UIParent;
		public RectTransform CardUIPoint;
		public RectTransform CardUIHidePoint;
		
		
		public AUXData[] Cards;
		public GameObject[] CardsGameObjects;
		public EntityStateMachine Machine;
		public int CurrentCardIndex;
		InputEventData _inputEventData = new();
		AUXState _currentState;
		
		
		
		public void Awake () {
			Machine = GetComponent<EntityStateMachine>();
			Machine.OnStateSwitch += OnStateSwitch;
			CardsGameObjects = new GameObject[Cards.Length];
			
		}

		void Start () {
			Machine.SetState(EntityState.InstantiateState(Cards[CurrentCardIndex].StateType.type));
		}

		public void OnStateSwitch () {
			if ((Machine.currentState as AUXState) != null) {
				_currentState = Machine.currentState as AUXState;
				_currentState.CardDeck = this;
				
				GameObject card = null;
				if (!CardsGameObjects[CurrentCardIndex]) {
					card = Instantiate(Cards[CurrentCardIndex].CardPrefab,  CardUIHidePoint.transform.position, Quaternion.identity, UIParent.transform);
					CardsGameObjects[CurrentCardIndex] = card;
				} else {
					card = CardsGameObjects[CurrentCardIndex];
				}
				card.transform.DOMove(CardUIPoint.transform.position, 1.5f).From(CardUIHidePoint.transform.position).SetEase(Ease.OutBack);
				
				if (CurrentCardIndex > 0) {
					CardsGameObjects[CurrentCardIndex - 1].transform.DOMove(CardUIHidePoint.transform.position, 1.5f).SetEase(Ease.InBack);
				}
				else {
					// there's a ? operator here because on the first card, the previous card is null
					CardsGameObjects[^1]?.transform.DOMove(CardUIHidePoint.transform.position, 1.5f).SetEase(Ease.InBack);
				}

			}
		}

		public void Click (InputAction.CallbackContext callbackContext) {
			_inputEventData.UpdateKeyState(callbackContext);
		}

		public void Update () {
			if (_inputEventData.down) {
				_currentState.OnClick();
				CurrentCardIndex++;
				if (CurrentCardIndex >= Cards.Length) {
					CurrentCardIndex = 0;
				}
				Machine.SetStateInterrupt(EntityState.InstantiateState(Cards[CurrentCardIndex].StateType.type));
			}
		}

		public void ForceSwitch () {
			Machine.SetState(EntityState.InstantiateState(Cards[CurrentCardIndex].StateType.type));
		}

		public void OnGUI () {
			GUI.Label(new Rect(0, 0, 100, 100), Cards[CurrentCardIndex].Name);
		}

		public void LateUpdate () {
			_inputEventData.RefreshKeyData();
		}
	}
}
