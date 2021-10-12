using Bangboom.Player.Input;
using Photon.Pun;
using UnityEngine;

namespace Bangboom.StateMachine
{
	public class MovementStateMachine : StateMachine
	{
		[HideInInspector] public Idle IdleState;
		[HideInInspector] public Moving MovingState;

		[Header("Player Logic")]
		public InputReader InputReader;
		public Rigidbody2D RigidBody2D;
		public PhotonView View;
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
