using BangBoom.Config;
using UnityEngine;

namespace Bangboom.StateMachine
{
	public class Idle : BaseState
	{
		private readonly MovementStateMachine movementStateMachine;
		
		public Idle(StateMachine stateMachine) : base("Idle", stateMachine)
		{
			movementStateMachine = stateMachine as MovementStateMachine;
		}

		public override void Enter()
		{
			base.Enter();
			movementStateMachine.Animator.SetBool(AnimationData.IsWalkingParam, false);
			movementStateMachine.RigidBody2D.velocity = Vector2.zero;
		}

		public override void OnUpdateLogic()
		{
			base.OnUpdateLogic();

			var movementDirection = movementStateMachine.MovementDirection;

			if(IsInputDoesNotEqualZero(movementDirection.x) || IsInputDoesNotEqualZero(movementDirection.y))
			{
				StateMachine.ChangeState(movementStateMachine.MovingState);
			}
		}

		private bool IsInputDoesNotEqualZero(float input) => Mathf.Abs(input) > Mathf.Epsilon;
	}
}
