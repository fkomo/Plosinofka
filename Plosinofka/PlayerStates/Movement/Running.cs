
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Entities;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game.Entities;

namespace Ujeby.Plosinofka.Game.PlayerStates
{
	class Running : PlayerMovementState
	{
		public override PlayerMovementStateEnum AsEnum => PlayerMovementStateEnum.Running;

		public override PlayerAnimations Animation
		{
			get
			{
				if (!Freeze)
				{
					if (Direction.X > 0)
						return PlayerAnimations.RunningRight;

					if (Direction.X < 0)
						return PlayerAnimations.RunningLeft;
				}

				return PlayerAnimations.Idle;
			}
		}

		private const double RunningStep = BaseStep * 2;

		public Running(PlayerMovementState currentState) : base(currentState)
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
				else if (button == Settings.Instance.InputMappings.Run)
					player.ChangeMovement(new Walking(this));
			}
		}

		public override void Update(Player0 player)
		{
			base.Update(player);
			
			if (!player.ObstacleAt(Side.Down))
				player.ChangeMovement(new Falling(this));

			else
				player.Velocity.X = Freeze ? 0 : Direction.X * RunningStep;
		}
	}
}
