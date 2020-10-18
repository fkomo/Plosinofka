using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka
{
	class Crouching : Standing
	{
		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Crouching; } }

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Released)
			{
				if (button == Settings.Current.PlayerControls.Crouch)
					player.ChangeState(new Standing());
			}
			else if (state == InputButtonState.Pressed)
			{
				if (button == InputButton.Left || button == InputButton.Right)
					player.ChangeState(new Sneaking(button));
			}
		}

		public override void Update(Player player, IRayCasting environment)
		{
			base.Update(player, environment);
		}
	}
}
