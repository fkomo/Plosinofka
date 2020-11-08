using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Entities;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game.Graphics;
using Ujeby.Plosinofka.Game.PlayerStates;

namespace Ujeby.Plosinofka.Game.Entities
{
	public enum PlayerDecals
	{
		DustParticlesRight,
		DustParticlesLeft,

		Count
	}

	public class Player0 : Player, ITrackable
	{
		// TODO melee attack
		// TODO directional shooting

		private readonly PlayerAction Action = new PlayerAction();
		private readonly PlayerMovement Movement = new PlayerMovement();

		public PlayerMovementStateEnum AllowedMovement =
			PlayerMovementStateEnum.Crouching |
			PlayerMovementStateEnum.Sneaking |
			PlayerMovementStateEnum.Walking |
			PlayerMovementStateEnum.Jumping |
			PlayerMovementStateEnum.Running |
			PlayerMovementStateEnum.Dashing |
			PlayerMovementStateEnum.Falling |
			PlayerMovementStateEnum.Diving |
			PlayerMovementStateEnum.Idle;

		private readonly string DefaultSpriteId;

		internal Player0(string name)
		{
			Name = name;

			var sprite = SpriteCache.LoadSprite($".\\Content\\Player\\player.png");
			DefaultSpriteId = sprite?.Id;

			var dataSprite = SpriteCache.LoadSprite($".\\Content\\Player\\player-data.png");
			if (dataSprite != null)
				BoundingBox = AABB.Union(AABB.FromMap(dataSprite, Level.ObstacleMask));

			else if (sprite != null)
				BoundingBox = new AABB(Vector2f.Zero, sprite.Size);

			ChangeMovement(new Idle());
		}

		public override void Render(AABB view, double interpolation)
		{
			var position = InterpolatedPosition(interpolation);

			var animation = SpriteCache.Get(Movement.Current.Animation.ToString());
			if (animation != null)
			{
				var frames = animation.Size.X / animation.Size.Y;
				Renderer.Instance.RenderSprite(view, animation, position, frame:Movement.Current.AnimationFrame % frames);
			}
			else
				// animation not found, use default sprite
				Renderer.Instance.RenderSprite(view, SpriteCache.Get(DefaultSpriteId), position);
		}

		public override void HandleButton(InputButton button, InputButtonState state)
		{
			Movement.Current?.HandleButton(button, state, this);
		}

		public override void Update(IEnvironment env)
		{
			// add dust particles effect when motion direction is changed
			if (StandingOnGround(env))
			{ 
				if (PreviousVelocity.X > 0 && !(Velocity.X > 0))
					Simulation.Instance.AddEntity(new Decal(SpriteCache.Get(PlayerDecals.DustParticlesRight.ToString()), 
						new Vector2f(Position.X + BoundingBox.Right, Position.Y)));

				else if (PreviousVelocity.X < 0 && !(Velocity.X < 0))
					Simulation.Instance.AddEntity(new Decal(SpriteCache.Get(PlayerDecals.DustParticlesLeft.ToString()),
						new Vector2f(Position.X + BoundingBox.Left, Position.Y)));
			}

			// save state before update
			PreviousPosition = Position;
			PreviousVelocity = Velocity;

			// update player according to his state and set new moving vector
			Movement.Current?.Update(this, env);
		}

		internal bool StandingOnGround(IEnvironment env)
		{
			var bb = BoundingBox + Position;
			return env.Overlap(new AABB(new Vector2f(bb.Left + 1, bb.Bottom - 1), new Vector2f(bb.Right - 1, bb.Bottom)));
		}

		/// <summary>
		/// change current movement state (effective on next update)
		/// </summary>
		/// <param name="newState"></param>
		internal void ChangeMovement(PlayerMovementState newState, bool pushCurrentState = true)
		{
			if (AllowedMovement.HasFlag(newState.AsEnum))
				Movement.Change(Movement.Current, newState, pushCurrentState);
		}

		/// <summary>
		/// add new movement state to stack (effective if no next state is defined)
		/// </summary>
		/// <param name="nextState"></param>
		internal void AddMovement(PlayerMovementState nextState)
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
