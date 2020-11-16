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

	public class Player0 : Player, ITrack
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

			var sprite = SpriteCache.LoadSprite(Program.ContentDirectory + $"Player\\player.png");
			DefaultSpriteId = sprite?.Id;

			var dataSprite = SpriteCache.LoadSprite(Program.ContentDirectory + $"Player\\player-data.png");
			if (dataSprite != null)
				BoundingBox = AABB.Union(AABB.FromMap(dataSprite, Level.ObstacleMask));

			else if (sprite != null)
				BoundingBox = new AABB(Vector2f.Zero, sprite.Size);

			ChangeMovement(new Idle());
		}

		public override string ToString() => $"{ Name }: { Position }, { Movement.Current }";

		public override void Render(AABB view, double interpolation)
		{
			var position = InterpolatedPosition(interpolation);

			var animation = SpriteCache.Get(Movement.Current.Animation.ToString());
			if (animation != null)
			{
				Renderer.Instance.RenderSpriteFrame(view, animation, position,
					Movement.Current.AnimationFrame % (animation.Size.X / animation.Size.Y));
			}
			else
				// animation not found, use default sprite
				Renderer.Instance.RenderSprite(view, SpriteCache.Get(DefaultSpriteId), position);
		}

		public override void HandleButton(InputButton button, InputButtonState state)
		{
			Movement.Current?.HandleButton(button, state, this);
		}

		public override void Update()
		{
			// add dust particles effect when motion direction is changed
			if (ObstacleAt(Side.Down))
			{ 
				if (PreviousVelocity.X > 0 && !(Velocity.X > 0))
					Simulation.Instance.AddEntity(new Decal(SpriteCache.Get(PlayerDecals.DustParticlesRight.ToString()), 
						new Vector2f(Position.X + BoundingBox.Right, Position.Y)));

				else if (PreviousVelocity.X < 0 && !(Velocity.X < 0))
					Simulation.Instance.AddEntity(new Decal(SpriteCache.Get(PlayerDecals.DustParticlesLeft.ToString()),
						new Vector2f(Position.X + BoundingBox.Left, Position.Y)));
			}

			base.Update();

			// update player according to his state and set new moving vector
			Movement.Current?.Update(this);
			Action.Current?.Update(this);
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
