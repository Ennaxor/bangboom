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

		public override void OnUpdateLogic()
		{
			if(!movementStateMachine.View.IsMine)
			{
				return;
			}
			
			base.OnUpdateLogic();

			var movementDirection = movementStateMachine.InputReader.MovementDirection;

			if(IsInputMoreThanZero(movementDirection.x) || IsInputMoreThanZero(movementDirection.y))
			{
				StateMachine.ChangeState(movementStateMachine.MovingState);
			}
		}

		private bool IsInputMoreThanZero(float input) => Mathf.Abs(input) > Mathf.Epsilon;
	}
}
