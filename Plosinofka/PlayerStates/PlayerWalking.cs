using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	class PlayerWalking : PlayerState
	{
		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Walking; } }

		public Vector2f Direction;

		public PlayerWalking(Vector2f direction) => Direction = direction;

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Pressed)
			{
				// if down was pressed
				if (button == InputButton.Down)
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerSneaking(Direction));

				else if (button == Settings.Current.PlayerControls.Running)
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerRunning(Direction));

				else if (button == Settings.Current.PlayerControls.Jump)
				{
					var velocity = player.JumpingVelocity + Direction * player.WalkingStep;
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerJumping(velocity));
				}

				// if opposite direction was pressed while moving, freeze
				else if (Direction.X != 0 && (button == InputButton.Left || button == InputButton.Right))
					Direction.X = 0;
			}
			else if (state == InputButtonState.Released)
			{
				// if both directions are pressed and one is released, player should continue moving in the other one
				if (Direction.X == 0 && (button == InputButton.Right || button == InputButton.Left))
					Direction.X = button == InputButton.Right ? -1 : 1;

				// if the last direction of movement is released
				else if (button == InputButton.Right || button == InputButton.Left)
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerStanding());
			}
		}

		public override void Update(Player player)
		{
			player.Position += Direction * player.WalkingStep;
		}
	}
}
