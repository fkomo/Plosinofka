using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Core;
using System;
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka
{
	class Falling : PlayerMovementState
	{
		public override PlayerMovementStateEnum AsEnum => PlayerMovementStateEnum.Falling;

		private const double FallStep = BaseStep * 0.5;

		// TODO coyote time (allow jump/move? even after player is past the edge)

		public Falling() : base()
		{
		}

		public Falling(PlayerMovementState currentState) : base(currentState)
		{
		}

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Pressed)
			{
				// if direction was changed in air
				if (button == InputButton.Left || button == InputButton.Right)
				{
					// if opposite direction is pressed
					if ((button == InputButton.Left && Direction.X > 0) ||
						(button == InputButton.Right && Direction.X < 0))
					{
						Freeze = true;
						player.AddMovement(new Walking(this));
					}
					else
					{
						// set new direction
						Direction = new Vector2f(button == InputButton.Right ? 1 : -1, Direction.Y);
						player.AddMovement(new Walking(this));
					}
				}
				else if (button == Settings.Current.PlayerControls.Crouch)
				{
					player.ChangeMovement(new Diving(this), false);
				}
			}
			else if (state == InputButtonState.Released)
			{
				if (button == InputButton.Left || button == InputButton.Right)
				{
					if (Freeze)
					{
						// player should continue moving in the one that is still pressed
						Direction = new Vector2f(button == InputButton.Right ? -1 : 1, Direction.Y);
						Freeze = false;

						player.AddMovement(new Walking(this));
					}
					else
					{
						Direction = new Vector2f(0, Direction.Y);
						player.AddMovement(new Idle());
					}
				}
			}
		}

		public override void Update(Player player, IRayCasting environment)
		{
			if (player.Velocity.Y == 0 && player.StandingOnGround(environment))
				player.ChangeToPreviousMovement();

			else
			{
				// air control
				player.Velocity.X = Freeze ? 0 : Direction.X * FallStep;

				player.Velocity.Y = Math.Max((player.Velocity + Simulation.Gravity).Y, Simulation.TerminalFallingVelocity);
			}
		}
	}
}
