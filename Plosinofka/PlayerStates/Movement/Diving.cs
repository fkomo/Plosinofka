using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Core;
using System;
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka
{
	class Diving : PlayerMovementState
	{
		public override PlayerMovementStateEnum AsEnum => PlayerMovementStateEnum.Diving;

		private const double DiveStep = BaseStep * 0.5;

		public Diving(PlayerMovementState currentState) : base(currentState)
		{
		}

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Released)
			{
				if (button == Settings.Current.PlayerControls.Crouch)
				{
					player.ChangeMovement(new Falling(this), false);
				}
				else if (button == InputButton.Left || button == InputButton.Right)
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
				player.Velocity.X = Freeze ? 0 : Direction.X * DiveStep;

				player.Velocity.Y = Math.Max((player.Velocity + Simulation.Gravity).Y, Simulation.TerminalFallingVelocity);
			}
		}
	}
}
