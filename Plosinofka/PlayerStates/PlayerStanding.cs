using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	class PlayerStanding : PlayerState
	{
		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Standing; } }

		public PlayerStanding()
		{
			PlayerStateMachine.Clear();
		}

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Pressed)
			{
				if (button == InputButton.Left || button == InputButton.Right)
				{
					player.Velocity.X = button == InputButton.Left ? -player.WalkingStep : player.WalkingStep;
					player.CurrentState = PlayerStateMachine.Change(player.CurrentState, new PlayerWalking());
				}
				else if (button == Settings.Current.PlayerControls.Crouch)
				{
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerCrouching());
				}
				else if (button == Settings.Current.PlayerControls.Jump)
				{
					player.CurrentState = PlayerStateMachine.Change(this, new PlayerJumping());
					player.Velocity = player.JumpingVelocity;
				}
			}
		}

		public override void Update(Player player)
		{
			// just standing
		}
	}
}
