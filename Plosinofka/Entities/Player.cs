using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Entities
{
	public class Player : DynamicEntity, IRenderable, IHandleInput
	{
		// TODO melee attack
		// TODO directional shooting

		private PlayerAction Action = new PlayerAction();
		private PlayerMovement Movement = new PlayerMovement();

		private Guid[] Animations = new Guid[(int)PlayerAnimations.Count];

		public Player(string name)
		{
			Name = name;

			LoadSprites();

			ChangeMovement(new Idle());
		}

		/// <summary>
		/// load all player sprites
		/// </summary>
		private void LoadSprites()
		{
			var defaultSprite = ResourceCache.LoadAnimationSprite($".\\Content\\{ Name }.png");
			Animations[0] = defaultSprite.Id;

			var dataSprite = ResourceCache.LoadSprite($".\\Content\\{ Name }-data.png", true);
			if (dataSprite != null)
				BoundingBox = AABB.Union(AABB.FromMap(dataSprite, Level.ShadowCasterMask));
			else
				BoundingBox = new AABB(Vector2f.Zero, (Vector2f)defaultSprite.Size);

			for (var i = 1; i < (int)PlayerAnimations.Count; i++)
			{
				var type = (PlayerAnimations)i;
				var animationSprite = 
					ResourceCache.LoadAnimationSprite($".\\Content\\{ Name }-{ type.ToString().ToLower() }.png");
				
				if (animationSprite != null)
					Animations[i] = animationSprite.Id;
			}
		}

		public void Render(Camera camera, double interpolation)
		{
			var interpolatedPosition = InterpolatedPosition(interpolation);

			var animationSprite = 
				ResourceCache.Get<AnimationSprite>(Animations[(int)Movement.Current.AnimationIndex]);

			if (animationSprite != null)
				Renderer.Instance.RenderSpriteFrame(camera, interpolation,
					animationSprite, Movement.Current.Frame % animationSprite.Frames, interpolatedPosition);

			else
				Renderer.Instance.RenderSpriteFrame(camera, interpolation,
					ResourceCache.Get<AnimationSprite>(Animations[(int)PlayerAnimations.Default]),
					0, interpolatedPosition);
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
		}

		/// <summary>
		/// change current movement state (effective on next update)
		/// </summary>
		/// <param name="newState"></param>
		public void ChangeMovement(PlayerMovementState newState, bool pushCurrentState = true)
		{
			Movement.Change(Movement.Current, newState, pushCurrentState);
		}

		/// <summary>
		/// add new movement state to stack (effective if no next state is defined)
		/// </summary>
		/// <param name="nextState"></param>
		public void AddMovement(PlayerMovementState nextState)
		{
			Movement.Push(nextState);
		}

		/// <summary>
		/// change to previous movement state (first on stack)
		/// </summary>
		internal void ChangeToPreviousMovement()
		{
			ChangeMovement(Movement.Pop(), false);
		}
	}
}
