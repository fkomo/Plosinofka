using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Core;
using System;
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka
{
	class Falling : Moving
	{
		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Falling; } }

		public Falling() : this(new Vector2f())
		{

		}

		public Falling(Vector2f direction) : base(direction)
		{
		}

		public Falling(InputButton button) : base(button)
		{
		}

		public Falling(PlayerState previousState) : base(previousState as Moving)
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
						player.States.Push(new Walking(this));
					}
					else
					{
						// set new direction
						Direction = new Vector2f(button == InputButton.Right ? 1 : -1, Direction.Y);
						player.States.Push(new Walking(Direction));
					}
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

						player.States.Push(new Walking(Direction));
					}
					else
					{
						Direction = new Vector2f(0, Direction.Y);
						player.States.Clear();
					}
				}
			}
		}

		public override void Update(Player player)
		{
			Log.Add($"Falling.Update: freeze={ Freeze }; Direction.X={ Direction.X }; velocityY={ player.Velocity.Y }");

			player.Velocity += Simulation.Gravity;
			player.Velocity.Y = Math.Max(player.Velocity.Y, Simulation.TerminalFallingVelocity);

			// air control
			if (!Freeze && Direction.X != 0)
				player.Velocity.X = Direction.X * Player.AirStep;
			else
				player.Velocity.X = 0;
		}
	}
}
