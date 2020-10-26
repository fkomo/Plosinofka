using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Core;

namespace Ujeby.Plosinofka
{
	class Standing : PlayerState
	{
		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Standing; } }

		public Standing()
		{
		}

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Pressed)
			{
				if (button == InputButton.Left || button == InputButton.Right)
					player.ChangeState(new Walking(button));

				else if (button == Settings.Current.PlayerControls.Jump)
					player.ChangeState(new Jumping());

				else if (button == Settings.Current.PlayerControls.Crouch)
					player.ChangeState(new Crouching());
			}
		}

		public override void Update(Player player, IRayCasting environment)
		{
			// just standing
			player.Velocity.X = 0;

			if (!player.StandingOnGround(environment))
				player.ChangeState(new Falling());
		}
	}
}
