using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka
{
	class Sneaking : Moving
	{
		public Sneaking(Vector2f direction) : base(direction)
		{
		}

		public Sneaking(InputButton button) : base(button)
		{
		}

		public Sneaking(Moving currentState) : base(currentState)
		{
		}

		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Sneaking; } }

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Pressed)
			{
				// opposite direction was pressed while moving, freeze
				if (button == InputButton.Left || button == InputButton.Right)
					Freeze = true;
			}
			else if (state == InputButtonState.Released)
			{
				if (button == InputButton.Right || button == InputButton.Left)
				{
					if (Freeze)
					{
						Direction = new Vector2f(button == InputButton.Right ? -1 : 1, Direction.Y);
						Freeze = false;
					}
					else
						player.ChangeState(new Crouching());
				}
				else if (button == Settings.Current.PlayerControls.Crouch)
					player.ChangeState(new Walking(this));
			}
		}

		public override void Update(Player player, IRayCasting environment)
		{
			if (!player.StandingOnGround(player.BoundingBox, environment))
				player.ChangeState(new Falling(this));

			else if (!Freeze)
				player.Velocity.X = Direction.X * Player.SneakingStep;
		}
	}
}
