using UnityEngine;

namespace Bangboom.StateMachine
{
	public class Moving : BaseState
	{
		private float horizontalInput;
		
		public Moving(MovementStateMachine stateMachine) : base("Moving", stateMachine)
		{
		}
		
		public override void Enter()
		{
			base.Enter();
			horizontalInput = 0f;
		}

		public override void OnUpdateLogic()
		{
			base.OnUpdateLogic();
			if(IsHorizontalInputLessThanZero())
			{
				StateMachine.ChangeState(((MovementStateMachine) StateMachine).IdleState);
			}
		}

		private bool IsHorizontalInputLessThanZero() => Mathf.Abs(horizontalInput) < Mathf.Epsilon;


		public override void OnUpdatePhysics()
		{
			base.OnUpdatePhysics();
		}

		public override void Exit()
		{
			base.Exit();
		}
	}
}