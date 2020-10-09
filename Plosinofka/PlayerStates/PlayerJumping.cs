using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	class PlayerJumping : PlayerState
	{
		private Vector2f Velocity;

		public PlayerJumping(Vector2f velocity) => Velocity = velocity;

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
					if ((button == InputButton.Left && Velocity.X > 0) || (button == InputButton.Right && Velocity.X < 0))
					{
						Velocity.X = 0;
						PlayerStateMachine.Push(new PlayerWalking(new Vector2f(0, 0)));
					}
					else
					{
						// set new direction
						var direction = new Vector2f(button == InputButton.Left ? -1.0 : 1.0, 0);
						Velocity.X = direction.X * player.AirStep;

						PlayerStateMachine.Push(new PlayerWalking(direction));
					}
				}
			}
			else if (state == InputButtonState.Released)
			{
				// if both directions are pressed and one is released
				if (Velocity.X == 0 && (button == InputButton.Right || button == InputButton.Left))
				{
					// player should continue moving in the one that is still pressed
					var direction = new Vector2f(button == InputButton.Left ? 1 : -1, 0);
					Velocity.X = direction.X * player.AirStep;

					PlayerStateMachine.Push(new PlayerWalking(direction));
				}

				// if direction of movement was released
				else if (button == InputButton.Right || button == InputButton.Left)
				{
					Velocity.X = 0;
					PlayerStateMachine.Clear();
				}
			}
		}

		public override void Update(Player player)
		{
			player.Position += Velocity;

			Velocity += Simulation.Gravity;

			// TODO collision with world
			var ground = 1080 / 8 * 3;
			if (player.Bottom > ground)
			{
				// landing
				player.Position.Y = ground - player.Size.Height / 2;
				player.CurrentState = PlayerStateMachine.Change(this, PlayerStateMachine.Pop());
			}
		}
	}
}
