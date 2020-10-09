using SDL2;
using System;
using System.Drawing;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Entities
{
	class Player : Entity, IRender, IHandleInput
	{
		public double Top { get { return Position.Y - Size.Height / 2; } }
		public double Bottom { get { return Position.Y + Size.Height / 2; } }
		public double Left { get { return Position.X - Size.Width / 2; } }
		public double Right { get { return Position.X + Size.Width / 2; } }

		public readonly Size Size = new Size(32, 64);

		/// <summary></summary>
		public double WalkingStep { get { return 16; } }

		/// <summary></summary>
		public double SneakingStep { get { return WalkingStep / 2; } }

		/// <summary></summary>
		public double RunningStep { get { return WalkingStep * 2; } }

		/// <summary>side step while in air</summary>
		public double AirStep { get { return RunningStep; } }

		/// <summary>initial upwards jump velocity</summary>
		public readonly Vector2f JumpingVelocity = new Vector2f(0, -50);

		public PlayerState CurrentState { get; set; }

		public Player(string name)
		{
			Name = name;
			CurrentState = PlayerStateMachine.Change(null, new PlayerStanding());
		}

		private Player()
		{

		}

		public void Render(IntPtr renderer, Entity beforeUpdate, double interpolation)
		{
			var playerRectangle = new SDL.SDL_Rect
			{
				w = Size.Width,
				h = Size.Height
			};

			if (beforeUpdate == null)
			{
				playerRectangle.x = (int)Position.X;
				playerRectangle.y = (int)Position.Y;
			}
			else
			{
				playerRectangle.x = 
					(int)(beforeUpdate.Position.X + (Position.X - beforeUpdate.Position.X) * interpolation);
				playerRectangle.y = 
					(int)(beforeUpdate.Position.Y + (Position.Y - beforeUpdate.Position.Y) * interpolation);
			}

			playerRectangle.x -= Size.Width / 2;
			playerRectangle.y -= Size.Height / 2;

			var playerColor = Color.FromArgb(255, 255, 255); // standing
			switch (CurrentState.AsEnum)
			{
				case PlayerStateEnum.Walking: playerColor = Color.FromArgb(255, 0, 0); break;
				case PlayerStateEnum.Running: playerColor = Color.FromArgb(255, 0, 255); break;
				case PlayerStateEnum.Jumping: playerColor = Color.FromArgb(0, 0, 255); break;
				case PlayerStateEnum.Sneaking: playerColor = Color.FromArgb(255, 255, 0); break;
				case PlayerStateEnum.Crouching: playerColor = Color.FromArgb(0, 255, 0); break;
			}

			if (CurrentState.AsEnum == PlayerStateEnum.Sneaking || CurrentState.AsEnum == PlayerStateEnum.Crouching)
			{
				playerRectangle.h /= 2;
				playerRectangle.y += Size.Height / 2;
			}

			SDL.SDL_SetRenderDrawColor(renderer, playerColor.R, playerColor.G, playerColor.B, playerColor.A);
			SDL.SDL_RenderFillRect(renderer, ref playerRectangle);
		}

		public override void Update()
		{
			CurrentState?.Update(this);
		}

		public void HandleButton(InputButton button, InputButtonState state)
		{
			CurrentState?.HandleButton(button, state, this);
		}

		public override Entity Copy()
		{
			return new Player
			{
				CurrentState = CurrentState,
				Id = Id,
				Name = Name,
				Position = Position
			};
		}
	}
}
