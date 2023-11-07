#region

using System;
using UnityEngine;

#endregion
public struct ComponentCache {
	public Animator animator;
	//public RigBuilder rigBuilder;

	public ComponentCache (GameObject obj) {
		animator = obj.GetComponentInChildren<Animator>();
	}
}

namespace EntityStates {
	//deez nuts

	public class EntityStateMachine : MonoBehaviour {

		public SerializableEntityStateType initialStateType;

		public string machineName;

		public ComponentCache componentCache;

		public EntityState currentState;
		public EntityState nextState;
		public EntityState awaitState;

		public void Awake () {
			componentCache = new ComponentCache(gameObject);
			currentState = new Empty();
			currentState.stateMachine = this;
		}

		public void Start () {
			if (nextState != null) {
				SetState(nextState);
				return;
			}

			Type stateType = initialStateType.type;
			if (currentState is Empty && stateType != null && stateType.IsSubclassOf(typeof(EntityState))) {
				SetState(EntityState.InstantiateState(stateType));
			}
		}

		public void Update () {
			currentState.Update();
		}

		public void FixedUpdate () {
			if (nextState != null) {
				SetState(nextState);
			}
			if (awaitState != null) {
				SetState(awaitState);
				awaitState = null;
			}
			
			currentState.FixedUpdate();
		}

		public void LateUpdate () {
			currentState.LateUpdate();
		}

		void OnDestroy () {
			if (currentState != null) {
				currentState.OnExit();
				currentState = null;
			}
		}

		public bool HasNextState () {
			return nextState != null;
		}

		public void SetNextState (EntityState state) {
			nextState = state;
		}

		public void SetAwaitState (EntityState state) {
			awaitState = state;
		}
		
		public event Action OnStateSwitch; 
		public void SetState (EntityState newState) {
			if (newState != null) {
				nextState = null;
				newState.stateMachine = this;

				currentState?.OnExit();
				currentState = newState;
				currentState.OnEnter();
				OnStateSwitch?.Invoke();
			} else {
				Debug.LogWarning($"Tried to go into null state on GameObject {gameObject.name}!");
			}
			//insert network code
		}

		public void SetStateInterrupt (EntityState newState) {
			if (currentState.CanBeInterrupted() && newState != null) {
				nextState = null;
				newState.stateMachine = this;

				currentState.OnExit();
				currentState = newState;
				currentState.OnEnter();
			} else {
				Debug.LogWarning($"Tried to interrupt into null state on GameObject {gameObject.name}!");
			}
		}

		public void SetNextStateToNull () {
			nextState = EntityState.InstantiateState(typeof(Empty));
		}
	}
}
