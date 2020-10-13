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
			//if (state == InputButtonState.Released)
			//{
			//	if (button == Settings.Current.PlayerControls.Crouch)
			//		player.CurrentState = player.State.Change(this, new Standing());
			//}
			//else if (state == InputButtonState.Pressed)
			//{
			//	if (button == InputButton.Left || button == InputButton.Right)
			//	{
			//		player.Velocity.X = button == InputButton.Left ? -Player.SneakingStep : Player.SneakingStep;
			//		player.CurrentState = player.State.Change(this, new Sneaking());
			//	}
			//}
		}

		public override void Update(Player player)
		{
			// just crouching
		}
	}
}
