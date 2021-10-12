using Rewired;
using UnityEngine;

namespace Bangboom.Player.Input
{
	public class InputReader : MonoBehaviour
	{
		public int PlayerId = 0;
		public Vector2 MovementDirection { get; private set; }
		
		private Rewired.Player player;
		

		private void Awake()
		{
			player = ReInput.players.GetPlayer(PlayerId);
		}

		private void Update()
		{
			MovementDirection = GetMovementValue();
		}

		private Vector2 GetMovementValue()
		{
			return new Vector2(player.GetAxis("Horizontal"), player.GetAxis("Vertical"));
		}
	}
}
