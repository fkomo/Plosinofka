using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Core;
using System;
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka
{
	class Jumping : Moving
	{
		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Jumping; } }

		public Jumping() : this(new Vector2f())
		{

		}

		public Jumping(Vector2f direction) : base(direction)
		{
		}

		public Jumping(Moving currentState) : base(currentState)
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
						player.PushState(new Walking(this));
					}
					else
					{
						// set new direction
						Direction = new Vector2f(button == InputButton.Right ? 1 : -1, Direction.Y);
						player.PushState(new Walking(Direction));
					}
				}
				else if (button == Settings.Current.PlayerControls.Crouch)
				{
					// TODO dive
				}
				else if (button == Settings.Current.PlayerControls.Jump)
				{
					// TODO double jump
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

						player.PushState(new Walking(Direction));
					}
					else 
					{
						Direction = new Vector2f(0, Direction.Y);
						player.PushState(new Standing());
					}
				}
			}
		}

		private bool InAir = false;

		public override void Update(Player player, IRayCasting environment)
		{
			if (InAir && player.StandingOnGround(player.BoundingBox, environment))
				player.ChangeToPreviousState();

			else
			{
				if (!InAir)
				{
					player.Velocity.Y = player.JumpingVelocity.Y;
					InAir = true;
				}
				else
				{
					// air control
					if (!Freeze)
						player.Velocity.X = Direction.X * Player.AirStep;
				}

				player.Velocity.Y = Math.Max((player.Velocity + Simulation.Gravity).Y, Simulation.TerminalFallingVelocity);
			}
		}
	}
}
