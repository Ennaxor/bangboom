using UnityEngine;

namespace Bangboom.StateMachine
{
	public class BaseState
	{
		public readonly string Name;
		protected readonly StateMachine StateMachine;

		protected BaseState(string name, StateMachine stateMachine)
		{
			Name = name;
			StateMachine = stateMachine;
		}
		
		public virtual void Enter()
		{
			
		}
		
		public virtual void Exit()
		{
			
		}
		
		public virtual void OnUpdateLogic()
		{
			
		}
		
		public virtual void OnUpdatePhysics()
		{
			
		}
	}
}

