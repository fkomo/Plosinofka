using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Common;
using System.IO;

namespace Ujeby.Plosinofka
{
	abstract class Moving : PlayerState
	{
		protected Vector2i Direction;

		protected Moving(Vector2i direction) => Direction = direction;

		protected Moving(InputButton button) => Direction = GetDirection(button);

		public static Vector2i GetDirection(InputButton button) => new Vector2i(button == InputButton.Left ? -1 : 1, 0);
	}

	class Walking : Moving
	{
		public Walking(Vector2i direction) : base(direction)
		{
		}

		public Walking(InputButton button) : base(button)
		{
		}		

		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.Walking; } }

		public bool Freeze = false;

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{
			if (state == InputButtonState.Pressed)
			{
				// opposite direction was pressed while moving, freeze
				if (button == InputButton.Left || button == InputButton.Right)
					Freeze = true;

				else if (button == Settings.Current.PlayerControls.Jump)
					player.CurrentState = player.States.Change(this, new Jumping(Direction));

				//else if (button == Settings.Current.PlayerControls.Crouch)
				//	player.CurrentState = player.States.Change(this, new Sneaking(Direction));

				//else if (button == Settings.Current.PlayerControls.Running)
				//	player.CurrentState = player.States.Change(this, new Running(Direction));
			}
			else if (state == InputButtonState.Released)
			{
				if (button == InputButton.Right || button == InputButton.Left)
				{
					if (Freeze)
					{
						Direction.X = button == InputButton.Right ? -1 : 1;
						Freeze = false;
					}
					else
						player.CurrentState = player.States.Change(this, new Standing());
				}
			}
		}

		public override void Update(Player player)
		{
			Log.Add($"Walking.Update: freeze={ Freeze }; Direction.X={ Direction.X }");

			if (!Freeze)
				player.Velocity.X = Direction.X * Player.WalkingStep;
			else
				player.Velocity.X = 0;
		}
	}
}
