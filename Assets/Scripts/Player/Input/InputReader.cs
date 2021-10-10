using System;
using Rewired;
using UnityEngine;

namespace Bangboom.Player.Input
{
	public class InputReader : MonoBehaviour
	{
		[SerializeField] private int playerId = 0;
		
		private Rewired.Player player;
		
		public Vector2 MovementDirection
		{
			get => movementDirection;
			set => movementDirection = value;
		}
		private Vector2 movementDirection;
		
		private void Awake()
		{
			player = ReInput.players.GetPlayer(playerId);
		}

		private void Update()
		{
			movementDirection = GetMovementValue();
		}

		private Vector2 GetMovementValue()
		{
			return new Vector2(player.GetAxis("Horizontal"), player.GetAxis("Vertical"));
		}
	}
}
