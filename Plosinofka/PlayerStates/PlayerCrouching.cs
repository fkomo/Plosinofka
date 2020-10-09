using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka
{
	class PlayerCrouching : PlayerStanding
	{
		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Crouching; } }

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Released)
			{
				if (button == Settings.Current.PlayerControls.Crouch)
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerStanding());
			}
			else if (state == InputButtonState.Pressed)
			{
				if (button == InputButton.Left || button == InputButton.Right)
				{
					player.Velocity.X = button == InputButton.Left ? -player.SneakingStep : player.SneakingStep;
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerSneaking());
				}
			}
		}

		public override void Update(Player player)
		{
			// just crouching
		}
	}
}
