using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka
{
	class Walking : PlayerMovementState
	{
		public override PlayerMovementStateEnum AsEnum => PlayerMovementStateEnum.Walking;

		public override PlayerAnimations AnimationIndex 
			=> Direction.X > 0 ? PlayerAnimations.WalkingRight : 
			(Direction.X < 0 ? PlayerAnimations.WalkingLeft : PlayerAnimations.Idle);

		private const double WalkingStep = BaseStep * 0.8;

		public Walking(Vector2f direction) : base(direction)
		{
		}

		public Walking(InputButton button) : base(button)
		{
		}

		public Walking(PlayerMovementState currentState) : base(currentState)
		{
		}

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Pressed)
			{
				// opposite direction was pressed while moving, freeze
				if (button == InputButton.Left || button == InputButton.Right)
					Freeze = true;

				else if (button == Settings.Current.PlayerControls.Jump)
					player.ChangeMovement(new Jumping(this));

				else if (button == Settings.Current.PlayerControls.Crouch)
					player.ChangeMovement(new Sneaking(this));

				else if (button == Settings.Current.PlayerControls.Run)
					player.ChangeMovement(new Running(this));

				else if (button == Settings.Current.PlayerControls.Dash && !Freeze)
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

		public override void Update(Player player, IRayCasting environment)
		{
			base.Update(player, environment);
			
			if (!player.StandingOnGround(environment))
				player.ChangeMovement(new Falling(this));

			else
				player.Velocity.X = Freeze ? 0 : Direction.X * WalkingStep;
		}
	}
}
