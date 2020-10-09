using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	class PlayerWalking : PlayerState
	{
		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Walking; } }

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
				else if (button == Settings.Current.PlayerControls.Running)
				{
					player.Velocity.X = player.Velocity.X > 0 ? player.RunningStep : -player.RunningStep;
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerRunning());
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
				// if both directions are pressed and one is released, player should continue moving in the other one
				if (player.Velocity.X == 0 && (button == InputButton.Right || button == InputButton.Left))
					player.Velocity.X = button == InputButton.Right ? -player.WalkingStep : player.WalkingStep;

				// if the last direction of movement is released
				else if (button == InputButton.Right || button == InputButton.Left)
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerStanding());
			}
		}

		public override void Update(Player player)
		{
			player.Position.X += player.Velocity.X;
		}
	}
}
