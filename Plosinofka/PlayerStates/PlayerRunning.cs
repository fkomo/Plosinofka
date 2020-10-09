using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	class PlayerRunning : PlayerWalking
	{
		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Running; } }

		public PlayerRunning(Vector2f direction) : base(direction)
		{
		}

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Pressed)
			{
				// if crouch was pressed
				if (button == Settings.Current.PlayerControls.Crouch)
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerSneaking(Direction));

				else if (button == Settings.Current.PlayerControls.Jump)
				{
					var velocity = player.JumpingVelocity + Direction * player.RunningStep;
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerJumping(velocity));
				}

				// if opposite direction was pressed while moving, freeze
				else if (Direction.X != 0 && (button == InputButton.Left || button == InputButton.Right))
					Direction.X = 0;
			}
			else if (state == InputButtonState.Released)
			{
				// running released
				if (button == Settings.Current.PlayerControls.Running)
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerWalking(Direction));

				// if both directions are pressed and one is released, player should continue moving in the other one
				else if (Direction.X == 0 && (button == InputButton.Right || button == InputButton.Left))
					Direction.X = button == InputButton.Right ? -1 : 1;

				// if the last direction of movement is released
				else if (button == InputButton.Right || button == InputButton.Left)
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerStanding());
			}
		}

		public override void Update(Player player)
		{
			player.Position += Direction * player.RunningStep;
		}
	}
}
