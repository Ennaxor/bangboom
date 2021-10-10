using UnityEngine;

namespace Bangboom.StateMachine
{
	public class Moving : BaseState
	{
		private readonly MovementStateMachine movementStateMachine;
		
		public Moving(StateMachine stateMachine) : base("Moving", stateMachine)
		{
			movementStateMachine = stateMachine as MovementStateMachine;
		}

		public override void OnUpdateLogic()
		{
			if(!movementStateMachine.View.IsMine)
			{
				return;
			}
			
			base.OnUpdateLogic();
			
			var movementDirection = movementStateMachine.InputReader.MovementDirection;
			
			if(IsInputLessThanZero(movementDirection.x) && IsInputLessThanZero(movementDirection.y))
			{
				StateMachine.ChangeState(movementStateMachine.IdleState);
			}
		}
		
		private bool IsInputLessThanZero(float input) => Mathf.Abs(input) < Mathf.Epsilon;

		public override void OnUpdatePhysics()
		{
			if(!movementStateMachine.View.IsMine)
			{
				return;
			}
			
			base.OnUpdatePhysics();
			var movementDirection = movementStateMachine.InputReader.MovementDirection;
			movementStateMachine.RigidBody2D.velocity = movementDirection * movementStateMachine.Speed;
		}
	}
}