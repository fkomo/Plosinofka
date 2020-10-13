using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Core;
using System;
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka
{
	class Falling : PlayerState
	{
		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Falling; } }

		public Falling()
		{
		}

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			// TODO movement handling (left/right) while falling

			//if (state == InputButtonState.Pressed)
			//{
			//	if (button == InputButton.Left || button == InputButton.Right)
			//	{
			//		player.Velocity.X = button == InputButton.Left ? -Player.WalkingStep : Player.WalkingStep;
			//		player.CurrentState = player.State.Change(player.CurrentState, new Walking());
			//	}
			//	else if (button == Settings.Current.PlayerControls.Crouch)
			//	{
			//		player.CurrentState = player.State.Change(this, new Crouching());
			//	}
			//	else if (button == Settings.Current.PlayerControls.Jump)
			//	{
			//		player.CurrentState = player.State.Change(this, new Jumping());
			//		player.Velocity = player.JumpingVelocity;
			//	}
			//}
		}

		public override void Update(Player player)
		{
			Log.Add($"Falling.Update: velocityY={ player.Velocity.Y }");

			player.Velocity += Simulation.Gravity;
			// terminal velocity
			player.Velocity.Y = Math.Max(player.Velocity.Y, -32);
		}
	}
}
