
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Entities;
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

				else if (button == Settings.Instance.InputMappings.Jump)
					player.ChangeMovement(new Jumping());

				else if (button == Settings.Instance.InputMappings.Crouch)
					player.ChangeMovement(new Crouching());
			}
		}

		public override void Update(Player0 player)
		{
			base.Update(player);

			// nothing to do
			player.Velocity = Vector2f.Zero;

			if (!player.ObstacleAt(Side.Down))
				player.ChangeMovement(new Falling());
		}
	}
}
