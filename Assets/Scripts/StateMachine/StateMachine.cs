using System;
using UnityEngine;

namespace Bangboom.StateMachine
{
	public class StateMachine : MonoBehaviour
	{
		public bool CanUpdate { get; set; }
		
		private BaseState currentState;
		
		private void Start()
		{
			currentState = GetInitialState();
			currentState?.Enter();
		}

		protected virtual BaseState GetInitialState()
		{
			return null;
		}

		/// <summary>
		/// We update our logic layer in the normal update
		/// </summary>
		protected virtual void Update()
		{
			if(CanUpdate)
			{
				currentState?.OnUpdateLogic();
			}
		}

		/// <summary>
		/// We update our physics layer in the late update
		/// </summary>
		private void FixedUpdate()
		{
			if(CanUpdate)
			{
				currentState?.OnUpdatePhysics();
			}
		}

		public void ChangeState(BaseState newState)
		{
			currentState?.Exit();
			
			currentState = newState;
			currentState.Enter();
		}

		/// <summary>
		/// Debug info to show on what state we are currently in
		/// </summary>
		private void OnGUI()
		{
			var content = currentState != null ? currentState.Name : "No current state";
			GUILayout.Label($"<color='black'><size=40>{content}</size></color>");
		}
	}
}

