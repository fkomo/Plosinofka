using System;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game.Entities;

namespace Ujeby.Plosinofka.Game.PlayerStates
{
	class Falling : PlayerMovementState
	{
		public override PlayerMovementStateEnum AsEnum => PlayerMovementStateEnum.Falling;

		public override PlayerAnimations Animation
		{
			get
			{
				if (!Freeze)
				{
					if (Direction.X > 0)
						return PlayerAnimations.FallingRight;
				
					if (Direction.X < 0)
						return PlayerAnimations.FallingLeft;
				}

				return PlayerAnimations.Falling;
			}
		}

		private const double FallStep = BaseStep * 0.5;

		// TODO coyote time (allow jump/move? even after player is past the edge)

		public Falling() : base()
		{
		}

		public Falling(PlayerMovementState currentState) : base(currentState)
		{
		}

		public override void HandleButton(InputButton button, InputButtonState state, Player0 player)
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

		public override void Update(Player0 player, IRayCasting environment)
		{
			base.Update(player, environment);
			
			if (player.Velocity.Y == 0 && player.StandingOnGround(environment))
				player.ChangeToPreviousMovement();

			else
			{
				// air control
				player.Velocity.X = Freeze ? 0 : Direction.X * FallStep;
				player.Velocity.Y += Simulation0.Gravity.Y;
			}
		}
	}
}
