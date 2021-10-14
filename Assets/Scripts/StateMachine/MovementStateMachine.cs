using Bangboom.Player.Input;
using Photon.Pun;
using UnityEngine;

namespace Bangboom.StateMachine
{
	public class MovementStateMachine : StateMachine
	{
		[HideInInspector] public Vector2 MovementDirection;

		public TextMesh PlayerPositionText;
		public Idle IdleState;
		public Moving MovingState;
		
		[Header("Player Logic")]
		public InputReader InputReader;
		public Rigidbody2D RigidBody2D;
		public Animator Animator;
		public float Speed = 4f;
		[SerializeField] private PhotonView view;

		private void Awake()
		{
			InitializeStates();
			PlayerPositionText.text = "Soy " + view.InstantiationId;
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

		public void SetMovementDirection(Vector2 movementDirection)
		{
			MovementDirection = movementDirection;
		}
	}
}
