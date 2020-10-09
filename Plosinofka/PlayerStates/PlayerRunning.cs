using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	class PlayerRunning : PlayerWalking
	{
		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Running; } }

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Pressed)
			{
				// if crouch was pressed
				if (button == Settings.Current.PlayerControls.Crouch)
				{
					player.Velocity.X = player.Velocity.X > 0 ? player.SneakingStep : -player.SneakingStep;
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerSneaking());
				}
				else if (button == Settings.Current.PlayerControls.Jump)
				{
					player.Velocity = player.JumpingVelocity + player.Velocity;
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerJumping());
				}

				// if opposite direction was pressed while moving, freeze
				else if (player.Velocity.X != 0 && (button == InputButton.Left || button == InputButton.Right))
					player.Velocity.X = 0;
			}
			else if (state == InputButtonState.Released)
			{
				// running released
				if (button == Settings.Current.PlayerControls.Running)
				{
					player.Velocity.X = player.Velocity.X > 0 ? player.WalkingStep : -player.WalkingStep;
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerWalking());
				}
				// if both directions are pressed and one is released, player should continue moving in the other one
				else if (player.Velocity.X == 0 && (button == InputButton.Right || button == InputButton.Left))
					player.Velocity.X = button == InputButton.Right ? -player.RunningStep : player.RunningStep;

				// if the last direction of movement is released
				else if (button == InputButton.Right || button == InputButton.Left)
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerStanding());
			}
		}

		public override void Update(Player player)
		{
			base.Update(player);
		}
	}
}
