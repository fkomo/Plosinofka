using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Graphics;
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
		public readonly Vector2f JumpingVelocity = new Vector2f(0, 32);

		public PlayerState CurrentState;
		public PlayerStateMachine State { get; private set; } = new PlayerStateMachine();

		public Guid PlayerSpriteId { get; private set; }

		public Player(string name) : base(name)
		{
			var sprite = ResourceCache.LoadSprite(@".\Content\player.png");
			PlayerSpriteId = sprite.Id;

			BoundingBox = new BoundingBox
			{
				Size = sprite.Size,
			};

			CurrentState = State.Change(null, new Standing());

			BeforeUpdate = new Player(this);
		}

		/// <summary>
		/// create initial copy of player object
		/// </summary>
		/// <param name="player"></param>
		private Player(Player player) : base(player.Name)
		{
			BoundingBox = player.BoundingBox;
			Velocity = player.Velocity;
		}

		public void Render(Camera camera, double interpolation)
		{
			var interpolatedPosition = BeforeUpdate.Position + (Position - BeforeUpdate.Position) * interpolation;
			Renderer.Instance.RenderSprite(camera, 
				interpolatedPosition, ResourceCache.Get<Sprite>(PlayerSpriteId), 
				interpolation);
		}

		public override void Update(ICollisionSolver collisionSolver)
		{
			// save state before update
			BeforeUpdate.BoundingBox = BoundingBox;
			BeforeUpdate.Velocity = Velocity;

			CurrentState?.Update(this);

			// add gravity if in air
			if (Velocity.Y != 0)
			{
				Position += Velocity;
				Velocity += Simulation.Gravity;
			}

			if (collisionSolver.Solve(this, out Vector2f position, out Vector2f velocity))
			{
				Position = position;
				Velocity = velocity;

				CurrentState = State.Change(CurrentState, (velocity.X != 0) ? State.Pop() : new Standing());
			}
		}

		public void HandleButton(InputButton button, InputButtonState state)
		{
			CurrentState?.HandleButton(button, state, this);
		}
	}
}
