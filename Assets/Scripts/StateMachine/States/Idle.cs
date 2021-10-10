using UnityEngine;

namespace Bangboom.StateMachine
{
	public class Idle : BaseState
	{
		private float horizontalInput;
		
		public Idle(MovementStateMachine stateMachine) : base("Idle", stateMachine)
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
			horizontalInput = Input.GetAxis("Horizontal");

			if(IsHorizontalInputMoreThanZero())
			{
				StateMachine.ChangeState(((MovementStateMachine) StateMachine).MovingState);
			}
		}

		private bool IsHorizontalInputMoreThanZero() => Mathf.Abs(horizontalInput) > Mathf.Epsilon;

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
