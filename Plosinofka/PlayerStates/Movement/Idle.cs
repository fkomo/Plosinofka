using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Core;

namespace Ujeby.Plosinofka
{
	class Idle : PlayerMovementState
	{
		public override PlayerMovementStateEnum AsEnum => PlayerMovementStateEnum.Idle;

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Pressed)
			{
				if (button == InputButton.Left || button == InputButton.Right)
					player.ChangeMovement(new Walking(button));

				else if (button == Settings.Current.PlayerControls.Jump)
					player.ChangeMovement(new Jumping());

				else if (button == Settings.Current.PlayerControls.Crouch)
					player.ChangeMovement(new Crouching());
			}
		}

		public override void Update(Player player, IRayCasting environment)
		{
			// nothing to do
			player.Velocity.X = 0;

			if (!player.StandingOnGround(environment))
				player.ChangeMovement(new Falling());
		}
	}
}
