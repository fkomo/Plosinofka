using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	class PlayerSneaking : PlayerWalking
	{
		public PlayerSneaking(Vector2f direction) : base(direction)
		{
		}

		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Sneaking; } }

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Pressed)
			{
				// if opposite direction was pressed while moving, freeze
				if (Direction.X != 0 && (button == InputButton.Left || button == InputButton.Right))
					Direction.X = 0;
			}
			if (state == InputButtonState.Released)
			{
				// if both directions are pressed and one is released, player should continue moving in the other one
				if (Direction.X == 0 && (button == InputButton.Right || button == InputButton.Left))
					Direction.X = button == InputButton.Right ? -1 : 1;

				// if the last direction of movement is released
				else if (button == InputButton.Right || button == InputButton.Left)
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerCrouching());

				// if crouch was released
				else if (button == Settings.Current.PlayerControls.Crouch)
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerWalking(Direction));
			}
		}

		public override void Update(Player player)
		{
			player.Position += Direction * player.SneakingStep;
		}
	}
}
