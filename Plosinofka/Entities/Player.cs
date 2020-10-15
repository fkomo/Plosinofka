using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Entities
{
	class Player : Entity, IRenderable, IHandleInput
	{
		public const double WalkingStep = 16;
		public const double SneakingStep = WalkingStep / 2;
		public const double RunningStep = WalkingStep * 2;
		public const double AirStep = WalkingStep;
		/// <summary>initial upwards jump velocity</summary>
		public readonly Vector2f JumpingVelocity = new Vector2f(0, 48);

		public PlayerState CurrentState;
		public PlayerStateMachine States { get; private set; } = new PlayerStateMachine();

		public Guid PlayerSpriteId { get; private set; }

		public Player(string name) : base(name)
		{
			var sprite = ResourceCache.LoadSprite(@".\Content\player.png");
			PlayerSpriteId = sprite.Id;

			BoundingBox = new BoundingBox
			{
				Size = (Vector2f)sprite.Size,
			};

			CurrentState = States.Change(null, new Standing());

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

			Renderer.Instance.RenderLine(camera, 
				interpolatedPosition + Size / 2, interpolatedPosition + Size / 2 + Velocity, 
				interpolation);
		}

		public void HandleButton(InputButton button, InputButtonState state)
		{
			CurrentState?.HandleButton(button, state, this);
		}

		public override void Update(ICollisionSolver collision, IRayCasting level)
		{
			// save state before update
			BeforeUpdate.BoundingBox = BoundingBox;
			BeforeUpdate.Velocity = Velocity;

			// update player, set velocity for new position
			CurrentState?.Update(this);

			// resolve collisions
			if (collision.Solve(this, out Vector2f position, out Vector2f velocity))
			{
				// collision found, use new position / velocity
				Position = position;
				Velocity = velocity;

				// landed on ground from fall/jump so change to previous state
				if (velocity.Y == 0 && (CurrentState is Falling || CurrentState is Jumping))
					CurrentState = States.Change(CurrentState, States.Pop(), false);
			}
			else
				// no collisions, add velocity
				Position += Velocity;

			// if not falling / jumping
			if (!(CurrentState is Falling) && !(CurrentState is Jumping))
			{
				// if velocity is pointing down or no ground beneath the feet
				if (Velocity.Y < 0 || !GroundBeneathMyFeet(level))
					CurrentState = States.Change(CurrentState, new Falling(CurrentState));
			}
		}

		private bool GroundBeneathMyFeet(IRayCasting level)
		{
			var leftFoot = level.Intersect(Position, Vector2f.Down, out Vector2f n1);
			var rightFoot = level.Intersect(new Vector2f(BoundingBox.Right, Position.Y), Vector2f.Down, out Vector2f n2);

			return leftFoot == 0 || rightFoot == 0;
		}
	}
}
