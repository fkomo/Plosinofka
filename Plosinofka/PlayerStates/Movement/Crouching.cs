
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Entities;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game.Entities;

namespace Ujeby.Plosinofka.Game.PlayerStates
{
	class Crouching : PlayerMovementState
	{
		public override PlayerMovementStateEnum AsEnum => PlayerMovementStateEnum.Crouching;

		public override PlayerAnimations Animation => PlayerAnimations.Crouching;

		public override void HandleButton(InputButton button, InputButtonState state, Player0 player)
		{
			if (state == InputButtonState.Released)
			{
				if (button == Settings.Instance.InputMappings.Crouch)
					player.ChangeMovement(new Idle());
			}
			else if (state == InputButtonState.Pressed)
			{
				if (button == InputButton.Left || button == InputButton.Right)
					player.ChangeMovement(new Sneaking(button));
			}
		}

		public override void Update(Player0 player)
		{
			base.Update(player);

			// nothing to do
			player.Velocity.X = 0;

			if (!player.ObstacleAt(Side.Down))
				player.ChangeMovement(new Falling());

			// TODO move camera slightly down
		}
	}
}
