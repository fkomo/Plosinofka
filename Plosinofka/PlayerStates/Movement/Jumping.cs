using System;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Entities;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game.Entities;

namespace Ujeby.Plosinofka.Game.PlayerStates
{
	class Jumping : PlayerMovementState
	{
		public override PlayerMovementStateEnum AsEnum => PlayerMovementStateEnum.Jumping;

		public override PlayerAnimations Animation
		{
			get
			{
				if (!Freeze)
				{
					if (Direction.X > 0)
						return PlayerAnimations.JumpingRight;

					if (Direction.X < 0)
						return PlayerAnimations.JumpingLeft;

					if (Direction.Y > 0)
						return PlayerAnimations.JumpingUp;
				}

				return PlayerAnimations.Falling;
			}
		}

		private const double JumpImpulse = 10;
		private const double DoubleJumpMultiplier = 1;
		private const double AirStep = BaseStep * 0.8;

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
						player.AddMovement(new Walking(Direction));
					}
				}
				else if (button == Settings.Instance.InputMappings.Crouch)
				{
					player.ChangeMovement(new Diving(this), false);
				}
				else if (button == Settings.Instance.InputMappings.Jump)
				{
					if (ExtraJump > 0)
					{
						ExtraJump--;
						Jump = new Vector2f(0, JumpImpulse * DoubleJumpMultiplier);
					}
				}
				else if (button == Settings.Instance.InputMappings.Dash)
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
				else if (button == Settings.Instance.InputMappings.Jump)
				{
					if (player.Velocity.Y > 0)
						player.Velocity.Y = 0.0;
				}
				else if (button == Settings.Instance.InputMappings.Run)
				{
					RunMultiplier = 1.0;
					player.AddMovement(new Walking(this));
				}
			}
		}

		public override void Update(Player0 player)
		{
			base.Update(player);
			
			if (Jump.Y == 0 && player.ObstacleAt(Side.Down))
				player.ChangeToPreviousMovement();

			else
			{
				if (Jump != Vector2f.Zero)
				{
					player.Velocity = Jump;
					Jump = Vector2f.Zero;
				}

				// air control
				player.Velocity.X = Freeze ? 0 : Direction.X * AirStep * RunMultiplier;
				player.Velocity.Y += Gravity;
			}
		}
	}
}
