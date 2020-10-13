using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka
{
	class Running : Moving
	{
		public Running(Vector2f direction) : base(direction)
		{
		}

		public Running(InputButton button) : base(button)
		{
		}

		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Running; } }

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			//if (state == InputButtonState.Pressed)
			//{
			//	// if crouch was pressed
			//	if (button == Settings.Current.PlayerControls.Crouch)
			//	{
			//		player.Velocity.X = player.Velocity.X > 0 ? Player.SneakingStep : -Player.SneakingStep;
			//		player.CurrentState = player.State.Change(this, new Sneaking());
			//	}
			//	else if (button == Settings.Current.PlayerControls.Jump)
			//	{
			//		player.Velocity = player.JumpingVelocity + player.Velocity;
			//		player.CurrentState = player.State.Change(this, new Jumping());
			//	}

			//	// if opposite direction was pressed while moving, freeze
			//	else if (player.Velocity.X != 0 && (button == InputButton.Left || button == InputButton.Right))
			//		player.Velocity.X = 0;
			//}
			//else if (state == InputButtonState.Released)
			//{
			//	// running released
			//	if (button == Settings.Current.PlayerControls.Running)
			//	{
			//		player.Velocity.X = player.Velocity.X > 0 ? Player.WalkingStep : -Player.WalkingStep;
			//		player.CurrentState = player.State.Change(this, new Walking());
			//	}
			//	// if both directions are pressed and one is released, player should continue moving in the other one
			//	else if (player.Velocity.X == 0 && (button == InputButton.Right || button == InputButton.Left))
			//		player.Velocity.X = button == InputButton.Right ? -Player.RunningStep : Player.RunningStep;

			//	// if the last direction of movement is released
			//	else if (button == InputButton.Right || button == InputButton.Left)
			//	{
			//		player.Velocity.X = 0;
			//		player.CurrentState = player.State.Change(this, new Standing());
			//	}
			//}
		}

		public override void Update(Player player)
		{
			//base.Update(player);
		}
	}
}
