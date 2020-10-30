using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Entities
{
	public class Player : DynamicEntity, IRenderable, IHandleInput, ITrackable
	{
		// TODO melee attack
		// TODO directional shooting

		private PlayerAction Action = new PlayerAction();
		private PlayerMovement Movement = new PlayerMovement();

		public PlayerMovementStateEnum AllowedMovements =
			PlayerMovementStateEnum.Walking |
			PlayerMovementStateEnum.Jumping |
			PlayerMovementStateEnum.Running |
			PlayerMovementStateEnum.Crouching |
			PlayerMovementStateEnum.Sneaking |
			PlayerMovementStateEnum.Dashing |
			PlayerMovementStateEnum.Diving |
			PlayerMovementStateEnum.Falling |
			PlayerMovementStateEnum.Idle;

		private static Guid DefaultSprite;
		private static Guid DefaultDataSprite;
		private static Guid[] Animations = new Guid[(int)PlayerAnimations.Count];

		static Player()
		{
			LoadSprites();
		}

		public Player(string name)
		{
			Name = name;

			var dataSprite = ResourceCache.Get<Sprite>(DefaultDataSprite);
			if (dataSprite != null)
				BoundingBox = AABB.Union(AABB.FromMap(dataSprite, Level.ShadowCasterMask));
			else
				BoundingBox = new AABB(Vector2f.Zero, (Vector2f)ResourceCache.Get<Sprite>(DefaultSprite).Size);

			ChangeMovement(new Idle());
		}

		/// <summary>
		/// load all player sprites
		/// </summary>
		private static void LoadSprites()
		{
			DefaultSprite = ResourceCache.LoadAnimationSprite($".\\Content\\Player\\player1.png").Id;
			DefaultDataSprite = ResourceCache.LoadSprite($".\\Content\\Player\\player1-data.png").Id;

			for (var i = 1; i < (int)PlayerAnimations.Count; i++)
			{
				var id = ((PlayerAnimations)i).ToString().ToLower();
				var sprite = ResourceCache.LoadAnimationSprite($".\\Content\\Player\\player1-{ id }.png");
				if (sprite != null)
					Animations[i] = sprite.Id;
			}
		}

		public void Render(Camera camera, double interpolation)
		{
			var position = InterpolatedPosition(interpolation);

			var animation = ResourceCache.Get<AnimationSprite>(Animations[(int)Movement.Current.Animation]);
			if (animation != null)
				Renderer.Instance.RenderSpriteFrame(camera, interpolation,
					animation, Movement.Current.AnimationFrame % animation.Frames, position);

			else
				// animation not found, use default sprite
				Renderer.Instance.RenderSpriteFrame(camera, interpolation,
					ResourceCache.Get<AnimationSprite>(DefaultSprite), 0, position);
		}

		public void HandleButton(InputButton button, InputButtonState state)
		{
			Movement.Current?.HandleButton(button, state, this);
		}

		public override void Update(IRayCasting env)
		{
			// add dust particles effect when motion direction is changed
			if (StandingOnGround(env) && PreviousVelocity.X > 0 && !(Velocity.X > 0))
				World.Instance.AddEntity(new Decal(
						Decals.Library[DecalsEnum.DustParticlesRight], 
						new Vector2f(Position.X + BoundingBox.Right, Position.Y)));

			// save state before update
			PreviousPosition = Position;
			PreviousVelocity = Velocity;

			// update player according to his state and set new moving vector
			Movement.Current?.Update(this, env);
		}

		public bool StandingOnGround(IRayCasting env)
		{
			var bb = BoundingBox + Position;
			return 
				0 == env.Trace(bb.Min, Vector2f.Down, out Vector2f n1) ||
				0 == env.Trace(new Vector2f(bb.Max.X, bb.Bottom), Vector2f.Down, out Vector2f n2) ||
				0 == env.Trace(new Vector2f(bb.Left + bb.Size.X * 0.33, bb.Bottom), Vector2f.Down, out Vector2f n3) ||
				0 == env.Trace(new Vector2f(bb.Left + bb.Size.X * 0.66, bb.Bottom), Vector2f.Down, out Vector2f n4);
		}

		/// <summary>
		/// change current movement state (effective on next update)
		/// </summary>
		/// <param name="newState"></param>
		public void ChangeMovement(PlayerMovementState newState, bool pushCurrentState = true)
		{
			if (AllowedMovements.HasFlag(newState.AsEnum))
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

		public string TrackId() => Name;

		public TrackedData Track()
		{
			return new TrackedData
			{
				Position = Center,
				Velocity = Velocity
			};
		}
	}
}
