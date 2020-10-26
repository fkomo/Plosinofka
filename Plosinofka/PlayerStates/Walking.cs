using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka
{
	abstract class Moving : PlayerState
	{
		public Vector2f Direction { get; protected set; }
		public bool Freeze { get; protected set; } = false;

		protected Moving(Vector2f direction) => Direction = direction;

		protected Moving(InputButton button) => Direction = new Vector2f(button == InputButton.Left ? -1 : 1, 0);

		protected Moving(Moving previousState)
		{
			if (previousState != null)
			{
				Direction = previousState.Direction;
				Freeze = previousState.Freeze;
			}
		}
	}

	class Walking : Moving
	{
		public Walking(Vector2f direction) : base(direction)
		{
		}

		public Walking(InputButton button) : base(button)
		{
		}

		public Walking(PlayerState currentState) : base(currentState as Moving)
		{
		}

		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Walking; } }

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Pressed)
			{
				// opposite direction was pressed while moving, freeze
				if (button == InputButton.Left || button == InputButton.Right)
					Freeze = true;

				else if (button == Settings.Current.PlayerControls.Jump)
					player.ChangeState(new Jumping(this));

				else if (button == Settings.Current.PlayerControls.Crouch)
					player.ChangeState(new Sneaking(this));

				else if (button == Settings.Current.PlayerControls.Run)
					player.ChangeState(new Running(this));
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
						player.ChangeState(new Standing());
				}
			}
		}

		public override void Update(Player player, IRayCasting environment)
		{
			if (!player.StandingOnGround(environment))
				player.ChangeState(new Falling(this));

			else if (!Freeze)
				player.Velocity.X = Direction.X * Player.WalkingStep;
		}
	}
}
