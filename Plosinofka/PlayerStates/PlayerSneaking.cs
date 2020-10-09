using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	class PlayerSneaking : PlayerWalking
	{
		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Sneaking; } }

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Pressed)
			{
				// if opposite direction was pressed while moving, freeze
				if (player.Velocity.X != 0 && (button == InputButton.Left || button == InputButton.Right))
					player.Velocity.X = 0;
			}
			if (state == InputButtonState.Released)
			{
				// if both directions are pressed and one is released, player should continue moving in the other one
				if (player.Velocity.X == 0 && (button == InputButton.Right || button == InputButton.Left))
					player.Velocity.X = button == InputButton.Right ? -player.SneakingStep : player.SneakingStep;

				// if the last direction of movement is released
				else if (button == InputButton.Right || button == InputButton.Left)
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerCrouching());

				// if crouch was released
				else if (button == Settings.Current.PlayerControls.Crouch)
				{
					player.Velocity.X = player.Velocity.X > 0 ? player.WalkingStep : -player.WalkingStep;
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerWalking());
				}
			}
		}

		public override void Update(Player player)
		{
			base.Update(player);
		}
	}
}
