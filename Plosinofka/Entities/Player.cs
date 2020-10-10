using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Entities
{
	class Player : Entity, IRender, IHandleInput
	{
		public const double WalkingStep = 16;
		public const double SneakingStep = WalkingStep / 2;
		public const double RunningStep = WalkingStep * 2;
		public const double AirStep = WalkingStep;

		/// <summary>initial upwards jump velocity</summary>
		public readonly Vector2f JumpingVelocity = new Vector2f(0, 34);

		public PlayerState CurrentState;
		public PlayerStateMachine State { get; private set; } = new PlayerStateMachine();

		public Guid PlayerSpriteId { get; private set; }

		public override Vector2i Size => ResourceCache.Get<Sprite>(PlayerSpriteId).Size;

		public Player(string name) : base(name)
		{
			PlayerSpriteId = ResourceCache.LoadSprite(@".\Content\player.png").Id;
			CurrentState = State.Change(null, new Standing());
		}

		private Player()
		{

		}

		public void Render(Camera camera, Entity beforeUpdate, double interpolation)
		{
			// interpolate position
			var newPosition = beforeUpdate.Position + (Position - beforeUpdate.Position) * interpolation;

			Renderer.Instance.RenderSprite(camera, 
				newPosition, ResourceCache.Get<Sprite>(PlayerSpriteId), 
				interpolation);
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

			if (Position.Y < Size.Y)
			{
				Velocity.Y = 0;
				Position.Y = Size.Y;
			}
			
			if (Position.X < 0)
			{
				Velocity.X = 0;
				Position.X = 0;
				CurrentState = State.Change(CurrentState, new HitWall());
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
