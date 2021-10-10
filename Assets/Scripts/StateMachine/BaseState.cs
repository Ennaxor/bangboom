using UnityEngine;

namespace Bangboom.StateMachine
{
	public class BaseState
	{
		public string Name;
		protected StateMachine StateMachine;
		
		public BaseState(string name, StateMachine stateMachine)
		{
			this.Name = name;
			this.StateMachine = stateMachine;
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

