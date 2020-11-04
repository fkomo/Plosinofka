
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game.Entities;

namespace Ujeby.Plosinofka.Game.PlayerStates
{
	class Idle : PlayerMovementState
	{
		public override PlayerMovementStateEnum AsEnum => PlayerMovementStateEnum.Idle;

		public override PlayerAnimations Animation => PlayerAnimations.Idle;

		public override void HandleButton(InputButton button, InputButtonState state, Player0 player)
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

		public override void Update(Player0 player, IRayCasting environment)
		{
			base.Update(player, environment);

			// nothing to do
			player.Velocity.X = 0;

			if (!player.StandingOnGround(environment))
				player.ChangeMovement(new Falling());
		}
	}
}
