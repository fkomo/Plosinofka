using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Entities
{
	public class Player : Entity, IRenderable, IHandleInput
	{
		public const double WalkingStep = 8;
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

			// update player according to his state and set new moving vector
			CurrentState?.Update(this);
		}

		public override void AfterUpdate(bool collisionFound, IRayCasting rayCasting)
		{
			if (collisionFound)
			{
				// if landed on ground from fall/jump so change to previous state
				if (Velocity.Y == 0 && (CurrentState is Falling || CurrentState is Jumping))
					CurrentState = States.Change(CurrentState, States.Pop(), false);
			}

			// if not falling / jumping
			if (!(CurrentState is Falling) && !(CurrentState is Jumping))
			{
				// if velocity is pointing down or no ground beneath the feet
				if (Velocity.Y < 0 || !GroundBeneathHerFeet(rayCasting))
					CurrentState = States.Change(CurrentState, new Falling(CurrentState));
			}
		}

		private bool GroundBeneathHerFeet(IRayCasting rayCasting)
		{
			return
				rayCasting.Intersect(Position, Vector2f.Down, out Vector2f n1) == 0 ||
				rayCasting.Intersect(new Vector2f(BoundingBox.Right, Position.Y), Vector2f.Down, out Vector2f n2) == 0;
		}
	}
}
