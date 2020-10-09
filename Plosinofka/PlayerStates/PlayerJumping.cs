using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	class PlayerJumping : PlayerState
	{
		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Jumping; } }

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			// TODO double jump

			if (state == InputButtonState.Pressed)
			{
				// if direction was changed in air
				if (button == InputButton.Left || button == InputButton.Right)
				{
					// if opposite direction is pressed
					if ((button == InputButton.Left && player.Velocity.X > 0) || 
						(button == InputButton.Right && player.Velocity.X < 0))
					{
						player.Velocity.X = 0;
						PlayerStateMachine.Push(new PlayerWalking());
					}
					else
					{
						// set new direction
						player.Velocity.X = button == InputButton.Left ? -player.AirStep : player.AirStep;
						PlayerStateMachine.Push(new PlayerWalking());
					}
				}
			}
			else if (state == InputButtonState.Released)
			{
				// if both directions are pressed and one is released
				if (player.Velocity.X == 0 && (button == InputButton.Right || button == InputButton.Left))
				{
					// player should continue moving in the one that is still pressed
					player.Velocity.X = button == InputButton.Left ? player.AirStep : -player.AirStep;
					PlayerStateMachine.Push(new PlayerWalking());
				}

				// if direction of movement was released
				else if (button == InputButton.Right || button == InputButton.Left)
				{
					player.Velocity.X = 0;
					PlayerStateMachine.Clear();
				}
			}
		}

		public override void Update(Player player)
		{
			if (player.Velocity.Y == 0)
				player.CurrentState = PlayerStateMachine.Change(this, PlayerStateMachine.Pop());
		}
	}
}
