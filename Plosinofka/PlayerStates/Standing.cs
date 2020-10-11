using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Core;

namespace Ujeby.Plosinofka
{
	class Standing : PlayerState
	{
		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Standing; } }

		public Standing()
		{
			//player.State.Clear();
		}

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Pressed)
			{
				if (button == InputButton.Left || button == InputButton.Right)
				{
					player.Velocity.X = button == InputButton.Left ? -Player.WalkingStep : Player.WalkingStep;
					player.CurrentState = player.State.Change(player.CurrentState, new Walking());
				}
				else if (button == Settings.Current.PlayerControls.Crouch)
				{
					player.CurrentState = player.State.Change(this, new Crouching());
				}
				else if (button == Settings.Current.PlayerControls.Jump)
				{
					player.CurrentState = player.State.Change(this, new Jumping());
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
