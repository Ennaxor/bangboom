using Bangboom.Player.Input;
using Photon.Pun;
using UnityEngine;

namespace Bangboom.StateMachine
{
	public class MovementStateMachine : StateMachine
	{
		[HideInInspector] public Vector2 MovementDirection;
		
		public Idle IdleState;
		public Moving MovingState;
		
		[Header("Player Logic")]
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
