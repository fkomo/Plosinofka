using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Entities
{
	/// <summary>
	/// LS - moving
	/// RS - shooting in specified direction (single shot / auto)
	/// LB - 
	/// RB - dash (while jumping/walking)
	/// LT - running ?
	/// RT -  
	/// A - jump/double jump (jump + down = dive)
	/// B - 
	/// X - melee attack
	/// Y - super power
	/// </summary>
	public class Player : DynamicEntity, IRenderable, IHandleInput
	{
		public const double WalkingStep = 8;
		public const double SneakingStep = WalkingStep / 2;
		public const double RunningStep = WalkingStep * 2;
		public const double AirStep = WalkingStep;
		public readonly Vector2f JumpingVelocity = new Vector2f(0, 35);

		public PlayerState CurrentState { get; private set; }
		private PlayerStateMachine States = new PlayerStateMachine();

		public Guid PlayerSpriteId { get; private set; }

		public Player(string name)
		{
			Name = name;

			var sprite = ResourceCache.LoadSprite(@".\Content\player.png");
			PlayerSpriteId = sprite.Id;

			boundingBox = new BoundingBox(Position, Position + sprite.Size);

			ChangeState(new Falling());
		}

		public void Render(Camera camera, double interpolation)
		{
			var interpolatedPosition = PreviousPosition + (Position - PreviousPosition) * interpolation;

			Renderer.Instance.RenderSprite(camera,
				ResourceCache.Get<Sprite>(PlayerSpriteId),
				interpolatedPosition,
				interpolation);

			Renderer.Instance.RenderLine(camera,
				interpolatedPosition + Size / 2, interpolatedPosition + Size / 2 + Velocity,
				interpolation);
		}

		public void HandleButton(InputButton button, InputButtonState state)
		{
			CurrentState?.HandleButton(button, state, this);
		}

		public override void Update(IRayCasting environment)
		{
			// save state before update
			PreviousPosition = Position;
			PreviousVelocity = Velocity;

			// update player according to his state and set new moving vector
			CurrentState?.Update(this, environment);
		}

		public bool StandingOnGround(BoundingBox bb, IRayCasting environment)
		{
			return 
				environment.Trace(bb.Min, Vector2f.Down, out Vector2f n1) == 0 ||
				environment.Trace(new Vector2f(bb.Center.X, bb.Bottom), Vector2f.Down, out Vector2f n2) == 0 ||
				environment.Trace(new Vector2f(bb.Max.X, bb.Bottom), Vector2f.Down, out Vector2f n3) == 0;
		}

		/// <summary>
		/// change current state (effective on next update)
		/// </summary>
		/// <param name="newState"></param>
		public void ChangeState(PlayerState newState, bool pushCurrentState = true)
		{
			Log.Add($"Player.State={ newState }");
			CurrentState = States.Change(CurrentState, newState, pushCurrentState);
		}

		/// <summary>
		/// add new state to stack (effective if no next state is defined)
		/// </summary>
		/// <param name="nextState"></param>
		public void PushState(PlayerState nextState)
		{
			States.Push(nextState);
		}

		/// <summary>
		/// change to previous state (first on stack)
		/// </summary>
		internal void ChangeToPreviousState()
		{
			ChangeState(States.Pop(), false);
		}
	}
}
