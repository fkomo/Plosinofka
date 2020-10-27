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

		public Jumping() : base(Vector2f.Zero)
		{
		}

		public Jumping(PlayerMovementState currentState) : base(currentState)
		{
			if (currentState is Running)
				SpeedMultiplier = 2.0;
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
						player.PushMovementState(new Walking(this));
					}
					else
					{
						// set new direction
						Direction = new Vector2f(button == InputButton.Right ? 1 : -1, Direction.Y);
						player.PushMovementState(new Walking(Direction));
					}
				}
				else if (button == Settings.Current.PlayerControls.Crouch)
				{
					// TODO dive
				}
				else if (button == Settings.Current.PlayerControls.Jump)
				{
					if (ExtraJump > 0)
					{
						ExtraJump--;
						DoubleJump = true;
					}
				}
				else if (button == Settings.Current.PlayerControls.Dash)
				{
					if (!Freeze && Math.Abs(Direction.X) > 0)
						player.ChangeMovementState(new Dashing(this), false);
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

						player.PushMovementState(new Walking(Direction));
					}
					else
					{
						Direction = new Vector2f(0, Direction.Y);
						player.PushMovementState(new Idle());
					}
				}
				else if (button == Settings.Current.PlayerControls.Jump)
				{
					if (InAir && player.Velocity.Y > 0)
						player.Velocity.Y = 0.0;
				}
				else if (button == Settings.Current.PlayerControls.Run)
				{
					SpeedMultiplier = 1.0;
					player.PushMovementState(new Walking(this));
				}
			}
		}

		private bool InAir = false;

		/// <summary>number of extra jumps left (double/triple/... jump from air)</summary>
		private int ExtraJump = 1;
		private bool DoubleJump = false;

		private double SpeedMultiplier = 1.0;

		public override void Update(Player player, IRayCasting environment)
		{
			if (InAir && player.StandingOnGround(environment))
				player.ChangeToPreviousMovementState();

			else
			{
				if (!InAir)
				{
					player.Velocity.Y = Player.Jump;
					InAir = true;
				}
				else
				{
					if (DoubleJump)
					{
						player.Velocity.Y = Player.Jump;
						DoubleJump = false;
					}

					// air control
					player.Velocity.X = Freeze ? 0 : Direction.X * Player.AirStep * SpeedMultiplier;
				}

				player.Velocity.Y = Math.Max((player.Velocity + Simulation.Gravity).Y, Simulation.TerminalFallingVelocity);
			}
		}
	}
}
