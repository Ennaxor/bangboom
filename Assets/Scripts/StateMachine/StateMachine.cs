using System;
using UnityEngine;

namespace Bangboom.StateMachine
{
	public class StateMachine : MonoBehaviour
	{
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
		private void Update()
		{
			currentState?.OnUpdateLogic();
		}

		/// <summary>
		/// We update our physics layer in the late update
		/// </summary>
		private void FixedUpdate()
		{
			currentState?.OnUpdatePhysics();
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

