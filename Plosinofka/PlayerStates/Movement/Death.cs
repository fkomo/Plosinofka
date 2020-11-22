using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Game.Entities;

namespace Ujeby.Plosinofka.Game.PlayerStates
{
	class Death : PlayerMovementState
	{
		public override PlayerMovementStateEnum AsEnum => PlayerMovementStateEnum.Death;

		public override PlayerAnimations Animation => PlayerAnimations.Death;

		public override void HandleButton(InputButton button, InputButtonState state, Player0 player)
		{
		}

		public override void Update(Player0 player)
		{
			base.Update(player);

			// nothing to do
			player.Velocity = Vector2f.Zero;
		}
	}
}
