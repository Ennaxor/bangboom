using System;
using UnityEngine;

namespace Bangboom.StateMachine
{
	public class MovementStateMachine : StateMachine
	{
		[HideInInspector] public Idle IdleState;
		[HideInInspector] public Moving MovingState;

		private void Awake()
		{
			InitializeStates();
		}

		private void InitializeStates()
		{
			IdleState = new Idle(this);
			MovingState = new Moving(this);
		}

		protected override BaseState GetInitialState()
		{
			return IdleState;
		}
	}
}
