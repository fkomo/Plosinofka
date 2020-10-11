using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Core;

namespace Ujeby.Plosinofka
{
	class Walking : PlayerState
	{
		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Walking; } }

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Pressed)
			{
				// if crouch was pressed
				if (button == Settings.Current.PlayerControls.Crouch)
				{
					player.Velocity.X = player.Velocity.X > 0 ? Player.SneakingStep : -Player.SneakingStep;
					player.CurrentState = player.State.Change(this, new Sneaking());
				}
				else if (button == Settings.Current.PlayerControls.Running)
				{
					player.Velocity.X = player.Velocity.X > 0 ? Player.RunningStep : -Player.RunningStep;
					player.CurrentState = player.State.Change(this, new Running());
				}
				else if (button == Settings.Current.PlayerControls.Jump)
				{
					player.Velocity = player.JumpingVelocity + player.Velocity;
					player.CurrentState = player.State.Change(this, new Jumping());
				}

				// if opposite direction was pressed while moving, freeze
				else if (player.Velocity.X != 0 && (button == InputButton.Left || button == InputButton.Right))
					player.Velocity.X = 0;
			}
			else if (state == InputButtonState.Released)
			{
				// if both directions are pressed and one is released, player should continue moving in the other one
				if (player.Velocity.X == 0 && (button == InputButton.Right || button == InputButton.Left))
					player.Velocity.X = button == InputButton.Right ? -Player.WalkingStep : Player.WalkingStep;

				// if the last direction of movement is released
				else if (button == InputButton.Right || button == InputButton.Left)
				{
					player.Velocity.X = 0;
					player.CurrentState = player.State.Change(this, new Standing());
				}
			}
		}

		public override void Update(Player player)
		{
			player.BoundingBox.TopLeft.X += player.Velocity.X;
		}
	}
}
