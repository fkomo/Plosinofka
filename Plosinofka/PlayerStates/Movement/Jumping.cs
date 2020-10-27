using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Core;
using System;
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka
{
	class Jumping : PlayerMovementState
	{
		public override PlayerMovementStateEnum AsEnum => PlayerMovementStateEnum.Jumping;

		private const double JumpImpulse = 18;
		private const double DoubleJumpMultiplier = 1.2;
		private const double AirStep = BaseStep;

		private Vector2f Jump = new Vector2f(0, JumpImpulse);

		/// <summary>number of extra jumps left (double/triple/... jump from air)</summary>
		private int ExtraJump = 1;
		private double RunMultiplier = 1.0;

		public Jumping() : base(Vector2f.Zero)
		{
		}

		public Jumping(PlayerMovementState currentState) : base(currentState)
		{
			// if jumping while running
			if (currentState is Running)
				RunMultiplier = 2.0;
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
						player.AddMovement(new Walking(Direction));
					}
				}
				else if (button == Settings.Current.PlayerControls.Crouch)
				{
					player.ChangeMovement(new Diving(this), false);
				}
				else if (button == Settings.Current.PlayerControls.Jump)
				{
					if (ExtraJump > 0)
					{
						ExtraJump--;
						Jump = new Vector2f(0, JumpImpulse * DoubleJumpMultiplier);
					}
				}
				else if (button == Settings.Current.PlayerControls.Dash)
				{
					if (!Freeze && Math.Abs(Direction.X) > 0)
						player.ChangeMovement(new Dashing(this), false);
				}
			}
			else if (state == InputButtonState.Released)
			{
				if (button == InputButton.Right || button == InputButton.Left)
				{
					if (Freeze)
					{
						// player should continue moving in the one that is still pressed
						Direction = new Vector2f(button == InputButton.Right ? -1 : 1, Direction.Y);
						Freeze = false;

						player.AddMovement(new Walking(Direction));
					}
					else
					{
						Direction = new Vector2f(0, Direction.Y);
						player.AddMovement(new Idle());
					}
				}
				else if (button == Settings.Current.PlayerControls.Jump)
				{
					if (player.Velocity.Y > 0)
						player.Velocity.Y = 0.0;
				}
				else if (button == Settings.Current.PlayerControls.Run)
				{
					RunMultiplier = 1.0;
					player.AddMovement(new Walking(this));
				}
			}
		}

		public override void Update(Player player, IRayCasting environment)
		{
			if (Jump.Y == 0 && player.StandingOnGround(environment))
				player.ChangeToPreviousMovement();

			else
			{
				// NOTE possible problem, if double jump is pressed at high fall velocity, it makes no real difference
				// best double jump is from highest point (when velocity is 0)
				player.Velocity += Jump;
				Jump = Vector2f.Zero;

				// air control
				player.Velocity.X = Freeze ? 0 : Direction.X * AirStep * RunMultiplier;

				player.Velocity.Y = Math.Max((player.Velocity + Simulation.Gravity).Y, Simulation.TerminalFallingVelocity);
			}
		}
	}
}
