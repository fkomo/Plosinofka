
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game.Entities;

namespace Ujeby.Plosinofka.Game.PlayerStates
{
	class Dashing : PlayerMovementState
	{
		public override PlayerMovementStateEnum AsEnum => PlayerMovementStateEnum.Dashing;

		public override PlayerAnimations Animation
		{
			get
			{
				if (Direction.X > 0)
					return PlayerAnimations.DashingRight;

				if (Direction.X < 0)
					return PlayerAnimations.DashingLeft;

				// this should not happen
				return PlayerAnimations.Idle;
			}
		}

		private const double DashStep = BaseStep * 8;
		private const double DashEndThreshold = 1;

		public Dashing(PlayerMovementState currentState) : base(currentState)
		{
			CurrentDashVelocity = Direction * DashStep;
		}

		private Vector2f CurrentDashVelocity;

		public override void HandleButton(InputButton button, InputButtonState state, Player0 player)
		{
			// dashing is uncontrollable
			if (state == InputButtonState.Released)
			{
				if ((button == InputButton.Right && Direction.X > 0) || (button == InputButton.Left && Direction.X < 0))
					player.AddMovement(new Idle());

				else if (button == Settings.Instance.InputMappings.Run)
					player.AddMovement(new Walking(Direction.Normalize()));
			}
		}

		public override void Update(Player0 player, IEnvironment environment)
		{
			base.Update(player, environment);

			player.Velocity = CurrentDashVelocity;

			CurrentDashVelocity *= 0.5;
			if (player.Velocity.X < DashEndThreshold)
				player.ChangeToPreviousMovement();
		}
	}
}
