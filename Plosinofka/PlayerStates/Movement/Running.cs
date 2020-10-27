using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka
{
	class Running : PlayerMovementState
	{
		public override PlayerMovementStateEnum AsEnum => PlayerMovementStateEnum.Running;

		public Running(PlayerMovementState currentState) : base(currentState)
		{
		}

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Pressed)
			{
				// opposite direction was pressed while moving, freeze
				if (button == InputButton.Left || button == InputButton.Right)
					Freeze = true;

				else if (button == Settings.Current.PlayerControls.Jump)
					player.ChangeMovementState(new Jumping(this));

				else if (button == Settings.Current.PlayerControls.Crouch)
					player.ChangeMovementState(new Sneaking(this));
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
						player.ChangeMovementState(new Idle());
				}
				else if (button == Settings.Current.PlayerControls.Run)
					player.ChangeMovementState(new Walking(this));
			}
		}

		public override void Update(Player player, IRayCasting environment)
		{
			if (!player.StandingOnGround(environment))
				player.ChangeMovementState(new Falling(this));

			else
				player.Velocity.X = Freeze ? 0 : Direction.X * Player.RunningStep;
		}
	}
}
