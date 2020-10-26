using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Entities
{
	public class Player : DynamicEntity, IRenderable, IHandleInput
	{
		// TODO animations
		// TODO melee attack
		// TODO directional shooting

		public const double WalkingStep = 4;
		public const double SneakingStep = WalkingStep / 2;
		public const double RunningStep = WalkingStep * 2;
		public const double AirStep = WalkingStep;
		public const double Jump = 18;

		private PlayerAction Action = new PlayerAction();
		private PlayerMovement Movement = new PlayerMovement();

		public Guid PlayerSpriteId { get; private set; }
		public Guid PlayerDataSpriteId { get; private set; }

		public Player(string name)
		{
			Name = name;

			PlayerSpriteId = ResourceCache.LoadSprite(@".\Content\plosinofka-guy-color.png", true).Id;

			var dataSprite = ResourceCache.LoadSprite(@".\Content\plosinofka-guy-data.png", true);
			PlayerDataSpriteId = dataSprite.Id;

			BoundingBox = AABB.Union(AABB.FromMap(dataSprite, Level.ShadowCasterMask));

			ChangeMovementState(new Idle());
		}

		public void Render(Camera camera, double interpolation)
		{
			var interpolatedPosition = InterpolatedPosition(interpolation);

			Renderer.Instance.RenderSprite(camera,
				ResourceCache.Get<Sprite>(PlayerSpriteId),
				interpolatedPosition,
				interpolation);

			var center = interpolatedPosition + BoundingBox.Min + Size / 2;
			Renderer.Instance.RenderLine(camera, center, center + Velocity, Color4b.Red, interpolation);
		}

		public void HandleButton(InputButton button, InputButtonState state)
		{
			Movement.Current?.HandleButton(button, state, this);
		}

		public override void Update(IRayCasting environment)
		{
			// save state before update
			PreviousPosition = Position;
			PreviousVelocity = Velocity;

			// update player according to his state and set new moving vector
			Movement.Current?.Update(this, environment);
		}

		public bool StandingOnGround(IRayCasting environment)
		{
			var bb = BoundingBox + Position;

			return
				0 == environment.Trace(bb.Min, Vector2f.Down, out Vector2f n1) ||
				0 == environment.Trace(new Vector2f(bb.Max.X, bb.Bottom), Vector2f.Down, out Vector2f n2) ||
				0 == environment.Trace(new Vector2f(bb.Left + bb.Size.X * 0.33, bb.Bottom), Vector2f.Down, out Vector2f n3) ||
				0 == environment.Trace(new Vector2f(bb.Left + bb.Size.X * 0.66, bb.Bottom), Vector2f.Down, out Vector2f n4);
				//environment.Intersect(new Ray(bb.Min, Vector2f.Down, true), to: 1) ||
				//environment.Intersect(new Ray(new Vector2f(bb.Max.X, bb.Bottom), Vector2f.Down, true), to: 1) ||
				//environment.Intersect(new Ray(new Vector2f(bb.Left + bb.Size.X * 0.33, bb.Bottom), Vector2f.Down, true), to: 1) ||
				//environment.Intersect(new Ray(new Vector2f(bb.Left + bb.Size.X * 0.66, bb.Bottom), Vector2f.Down, true), to: 1);
		}

		/// <summary>
		/// change current movement state (effective on next update)
		/// </summary>
		/// <param name="newState"></param>
		public void ChangeMovementState(PlayerMovementState newState, bool pushCurrentState = true)
		{
			Movement.Change(Movement.Current, newState, pushCurrentState);
		}

		/// <summary>
		/// add new movement state to stack (effective if no next state is defined)
		/// </summary>
		/// <param name="nextState"></param>
		public void PushMovementState(PlayerMovementState nextState)
		{
			Movement.Push(nextState);
		}

		/// <summary>
		/// change to previous movement state (first on stack)
		/// </summary>
		internal void ChangeToPreviousMovementState()
		{
			ChangeMovementState(Movement.Pop(), false);
		}
	}
}
