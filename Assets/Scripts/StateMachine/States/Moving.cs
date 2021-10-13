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
			base.OnUpdateLogic();
			
			var movementDirection = movementStateMachine.MovementDirection;
			
			if(IsInputLessThanZero(movementDirection.x) && IsInputLessThanZero(movementDirection.y))
			{
				StateMachine.ChangeState(movementStateMachine.IdleState);
			}
		}
		
		private bool IsInputLessThanZero(float input) => Mathf.Abs(input) < Mathf.Epsilon;

		public override void OnUpdatePhysics()
		{
			base.OnUpdatePhysics();
			
			var movementDirection = movementStateMachine.MovementDirection;
			movementStateMachine.RigidBody2D.velocity = movementDirection * movementStateMachine.Speed;
		}
	}
}