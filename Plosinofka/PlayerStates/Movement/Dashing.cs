using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;

namespace Ujeby.Plosinofka
{
	class Dashing : PlayerMovementState
	{
		public override PlayerMovementStateEnum AsEnum => PlayerMovementStateEnum.Dashing;

		private const double DashStep = BaseStep * 4;
		private const double DashEndThreshold = 1;

		public Dashing(PlayerMovementState currentState) : base(currentState)
		{
			CurrentDashVelocity = Direction * DashStep;
		}

		private Vector2f CurrentDashVelocity;

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			// dashing is uncontrollable
			if (state == InputButtonState.Released)
			{
				if ((button == InputButton.Right && Direction.X > 0) || (button == InputButton.Left && Direction.X < 0))
					player.AddMovement(new Idle());

				else if (button == Settings.Current.PlayerControls.Run)
					player.AddMovement(new Walking(Direction.Normalize()));
			}
		}

		public override void Update(Player player, IRayCasting environment)
		{
			player.Velocity = CurrentDashVelocity;

			CurrentDashVelocity *= 0.5;
			if (player.Velocity.X < DashEndThreshold)
				player.ChangeToPreviousMovement();
		}
	}
}
