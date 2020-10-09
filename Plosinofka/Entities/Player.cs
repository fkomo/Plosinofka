using SDL2;
using System.Drawing;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Entities
{
	class Player : Entity, IRender, IHandleInput
	{
		public readonly Vector2i Size = new Vector2f(32, 64);

		/// <summary></summary>
		public double WalkingStep { get { return 16; } }

		/// <summary></summary>
		public double SneakingStep { get { return WalkingStep / 2; } }

		/// <summary></summary>
		public double RunningStep { get { return WalkingStep * 2; } }

		/// <summary>side step while in air</summary>
		public double AirStep { get { return WalkingStep; } }

		/// <summary>initial upwards jump velocity</summary>
		public readonly Vector2f JumpingVelocity = new Vector2f(0, 34);

		public PlayerState CurrentState { get; set; }

		public Player(string name)
		{
			Name = name;
			CurrentState = PlayerStateMachine.Change(null, new PlayerStanding());
		}

		private Player()
		{

		}

		public void Render(Camera camera, Entity beforeUpdate, double interpolation)
		{
			// interpolate
			var newPosition = beforeUpdate.Position + (Position - beforeUpdate.Position) * interpolation;

			// relative to camera
			newPosition = camera.RelateTo(newPosition, interpolation);

			// inverse y coord
			newPosition.Y = camera.Size.Y - newPosition.Y;

			RenderPlayer(newPosition);
		}

		private void RenderPlayer(Vector2i position)
		{
			// color based on player state
			var playerColor = Color.FromArgb(255, 255, 255);
			switch (CurrentState.AsEnum)
			{
				case PlayerStateEnum.Walking: playerColor = Color.FromArgb(255, 0, 0); break;
				case PlayerStateEnum.Running: playerColor = Color.FromArgb(0, 255, 0); break;
				case PlayerStateEnum.Jumping: playerColor = Color.FromArgb(0, 0, 255); break;
				case PlayerStateEnum.Sneaking: playerColor = Color.FromArgb(127, 0, 0); break;
				case PlayerStateEnum.Crouching: playerColor = Color.FromArgb(127, 127, 127); break;
			}

			// move to top left corner
			position -= Size / 2;

			// player shape
			var playerRectangle = new SDL.SDL_Rect
			{
				w = Size.X,
				h = Size.Y,
				x = position.X,
				y = position.Y,
			};

			if (CurrentState.AsEnum == PlayerStateEnum.Sneaking || CurrentState.AsEnum == PlayerStateEnum.Crouching)
			{
				playerRectangle.h /= 2;
				playerRectangle.y += Size.Y / 2;
			}

			var renderer = Renderer.Instance.RendererPtr;
			SDL.SDL_SetRenderDrawColor(renderer, playerColor.R, playerColor.G, playerColor.B, playerColor.A);
			SDL.SDL_RenderFillRect(renderer, ref playerRectangle);
		}

		public override void Update()
		{
			CurrentState?.Update(this);

			// add gravity if in air
			if (Velocity.Y != 0)
			{
				Position += Velocity;
				Velocity += Simulation.Gravity;
			}

			// TODO collide with world

			if (Position.Y < Size.Y / 2)
			{
				Velocity.Y = 0;
				Position.Y = Size.Y / 2;
			}
			
			if (Position.X < Size.X / 2)
			{
				Velocity.X = 0;
				Position.X = Size.X / 2;
				CurrentState = PlayerStateMachine.Change(CurrentState, new PlayerHitWall());
			}
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
