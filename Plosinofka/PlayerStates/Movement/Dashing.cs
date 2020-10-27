using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;

namespace Ujeby.Plosinofka
{
	class Dashing : PlayerMovementState
	{
		public override PlayerMovementStateEnum AsEnum => PlayerMovementStateEnum.Dashing;

		public Dashing(PlayerMovementState currentState) : base(currentState)
		{
			CurrentDashVelocity = Direction * DashStep;
		}

		private Vector2f CurrentDashVelocity;
		private const double DashStep = 32;
		private const double DashEndThreshold = 1;

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			// dashing is uncontrollable
			if (state == InputButtonState.Released)
			{
				if ((button == InputButton.Right && Direction.X > 0) || (button == InputButton.Left && Direction.X < 0))
					player.PushMovementState(new Idle());

				else if (button == Settings.Current.PlayerControls.Run)
					player.PushMovementState(new Walking(Direction.Normalize()));
			}
		}

		public override void Update(Player player, IRayCasting environment)
		{
			player.Velocity = CurrentDashVelocity;

			CurrentDashVelocity *= 0.5;
			if (player.Velocity.X < DashEndThreshold)
				player.ChangeToPreviousMovementState();
		}
	}
}
