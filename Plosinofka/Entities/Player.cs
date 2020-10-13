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
		public readonly Vector2f JumpingVelocity = new Vector2f(0, 64);

		public PlayerState CurrentState;
		public PlayerStateMachine States { get; private set; } = new PlayerStateMachine();

		public Guid PlayerSpriteId { get; private set; }

		public Player(string name) : base(name)
		{
			var sprite = ResourceCache.LoadSprite(@".\Content\player.png");
			PlayerSpriteId = sprite.Id;

			BoundingBox = new BoundingBox
			{
				Size = sprite.Size,
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
		}

		public void HandleButton(InputButton button, InputButtonState state)
		{
			CurrentState?.HandleButton(button, state, this);
		}

		public override void Update(ICollisionSolver collisionSolver)
		{
			// save state before update
			BeforeUpdate.BoundingBox = BoundingBox;
			BeforeUpdate.Velocity = Velocity;

			// update player, set velocity for new position
			CurrentState?.Update(this);

			//Log.Add($"Player.Update#BeforeSolve: position={ Position }; velocity={ Velocity }");

			// resolve collisions
			if (collisionSolver.Solve(this, out Vector2f position, out Vector2f velocity))
			{
				//Log.Add($"Player.Update#Solved: position={ position }; velocity={ velocity }");

				Position = position;
				Velocity = velocity;

				// landed on ground from fall/jump
				if (velocity.Y == 0 && (CurrentState is Falling || CurrentState is Jumping))
				{
					// TODO this does not work when i hit ceiling with player head
					CurrentState = States.Change(CurrentState, States.Pop(), false);
				}
			}
			else
				Position += Velocity;

			// if not falling / jumping
			if (!(CurrentState is Falling) && !(CurrentState is Jumping))
			{
				// if velocity is pointing down
				if (Velocity.Y < 0 || !GroundBeneathMyFeet())
					CurrentState = States.Change(CurrentState, new Falling());
			}
		}

		private bool GroundBeneathMyFeet()
		{
			// TODO check if there is ground beneath my feet (raycast down)

			return true;
		}
	}
}
