using System;
using Bangboom.Player.Input;
using UnityEngine;

namespace Bangboom.StateMachine
{
	public class MovementStateMachine : StateMachine
	{
		[HideInInspector] public Idle IdleState;
		[HideInInspector] public Moving MovingState;

		public InputReader InputReader;
		public Rigidbody2D RigidBody2D;
		public float Speed = 4f;

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
