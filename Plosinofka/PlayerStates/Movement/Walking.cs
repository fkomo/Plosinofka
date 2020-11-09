using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game.Entities;

namespace Ujeby.Plosinofka.Game.PlayerStates
{
	class Walking : PlayerMovementState
	{
		public override PlayerMovementStateEnum AsEnum => PlayerMovementStateEnum.Walking;

		public override PlayerAnimations Animation
		{
			get 
			{
				if (!Freeze)
				{
					if (Direction.X > 0)
						return PlayerAnimations.WalkingRight;

					if (Direction.X < 0)
						return PlayerAnimations.WalkingLeft;
				}

				return PlayerAnimations.Idle;
			}
		}

		private const double WalkingStep = BaseStep * 0.5;

		public Walking(Vector2f direction) : base(direction)
		{
		}

		public Walking(InputButton button) : base(button)
		{
		}

		public Walking(PlayerMovementState currentState) : base(currentState)
		{
		}

		public override void HandleButton(InputButton button, InputButtonState state, Player0 player)
		{
			if (state == InputButtonState.Pressed)
			{
				// opposite direction was pressed while moving, freeze
				if (button == InputButton.Left || button == InputButton.Right)
					Freeze = true;

				else if (button == Settings.Instance.InputMappings.Jump)
					player.ChangeMovement(new Jumping(this));

				else if (button == Settings.Instance.InputMappings.Crouch)
					player.ChangeMovement(new Sneaking(this));

				else if (button == Settings.Instance.InputMappings.Run)
					player.ChangeMovement(new Running(this));

				else if (button == Settings.Instance.InputMappings.Dash && !Freeze)
					player.ChangeMovement(new Dashing(this));
			}
			else if (state == InputButtonState.Released)
			{
				if (button == InputButton.Right || button == InputButton.Left)
				{
					if (Freeze)
					{
						Direction = new Vector2f(button == InputButton.Right ? -1 : 1, Direction.Y);
						Freeze = false;
					}
					else
						player.ChangeMovement(new Idle());
				}
			}
		}

		public override void Update(Player0 player, IEnvironment environment)
		{
			base.Update(player, environment);
			
			if (!player.StandingOnGround(environment))
				player.ChangeMovement(new Falling(this));

			else
				player.Velocity.X = Freeze ? 0 : Direction.X * WalkingStep;
		}
	}
}
