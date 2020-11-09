using System;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game.Entities;

namespace Ujeby.Plosinofka.Game.PlayerStates
{
	class Diving : PlayerMovementState
	{
		public override PlayerMovementStateEnum AsEnum => PlayerMovementStateEnum.Diving;

		public override PlayerAnimations Animation => PlayerAnimations.Diving;

		private const double DiveStep = BaseStep * 0.5;

		public Diving(PlayerMovementState currentState) : base(currentState)
		{
		}

		public override void HandleButton(InputButton button, InputButtonState state, Player0 player)
		{
			if (state == InputButtonState.Released)
			{
				if (button == Settings.Current.InputMappings.Crouch)
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

		public override void Update(Player0 player, IEnvironment environment)
		{
			base.Update(player, environment);
			
			if (player.Velocity.Y == 0 && player.StandingOnGround(environment))
				player.ChangeToPreviousMovement();

			else
			{
				// air control
				player.Velocity.X = Freeze ? 0 : Direction.X * DiveStep;
				player.Velocity.Y += Simulation0.Gravity.Y;
			}
		}
	}
}
