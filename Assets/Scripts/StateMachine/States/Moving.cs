using BangBoom.Config;
using UnityEngine;

namespace Bangboom.StateMachine
{
	public class Moving : BaseState
	{
		private readonly MovementStateMachine movementStateMachine;
		private bool facingRight = true;
		
		public Moving(StateMachine stateMachine) : base("Moving", stateMachine)
		{
			movementStateMachine = stateMachine as MovementStateMachine;
		}

		public override void Enter()
		{
			base.Enter();
			movementStateMachine.Animator.SetBool(AnimationData.IsWalkingParam, true);
		}

		private void CheckFacing()
		{
			var movementDirection = movementStateMachine.MovementDirection;
			var isMovingLeft = IsInputIsLessThanZero(movementDirection.x);
			
			if(!isMovingLeft && !facingRight)
			{
				Flip();
			} 
			else if(isMovingLeft && facingRight)
			{
				Flip();
			}
		}

		private void Flip()
		{
			facingRight = !facingRight;
			var transformLocalScale = movementStateMachine.RigidBody2D.transform.localScale;
			transformLocalScale.x *= -1;
			movementStateMachine.RigidBody2D.transform.localScale = transformLocalScale;
		}

		public override void OnUpdateLogic()
		{
			base.OnUpdateLogic();
			
			var movementDirection = movementStateMachine.MovementDirection;
			CheckFacing();
			
			if(IsInputEqualsZero(movementDirection.x) && IsInputEqualsZero(movementDirection.y))
			{
				StateMachine.ChangeState(movementStateMachine.IdleState);
			}
		}

		public override void OnUpdatePhysics()
		{
			base.OnUpdatePhysics();
			
			var movementDirection = movementStateMachine.MovementDirection;
			movementStateMachine.RigidBody2D.velocity = movementDirection * movementStateMachine.Speed;
		}
		
		private bool IsInputEqualsZero(float input) => Mathf.Abs(input) < Mathf.Epsilon;
		
		private bool IsInputIsLessThanZero(float input) => input < 0f;
	}
}