
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Game.Entities;

namespace Ujeby.Plosinofka.Game.PlayerStates
{
	public enum PlayerMovementStateEnum : int
	{
		Idle = 1,
		Crouching = 2,
		Walking = 4,
		Running = 8,
		Sneaking = 16,
		Jumping = 32,
		Falling = 64,
		Dashing = 128,
		Diving = 256,
		Death = 512,
	}

	public abstract class PlayerMovementState : State<PlayerMovementStateEnum>
	{
		protected const double Gravity = -1;

		protected const double BaseStep = 4;

		/// <summary>current animation frame</summary>
		public int AnimationFrame { get; protected set; } = 0;
		protected double LastFrameChange;
		/// <summary>desired delay betwen animation frames [ms]</summary>
		protected const double AnimationFrameDelay = 100;

		public abstract PlayerAnimations Animation { get; }

		public Vector2f Direction { get; protected set; }
		public bool Freeze { get; protected set; } = false;

		protected PlayerMovementState() : this(Vector2f.Zero)
		{
		}

		protected PlayerMovementState(Vector2f direction) => 
			Direction = direction;

		protected PlayerMovementState(InputButton button) => 
			Direction = new Vector2f(button == InputButton.Left ? -1 : 1, 0);

		protected PlayerMovementState(PlayerMovementState previousState)
		{
			if (previousState != null)
			{
				Direction = previousState.Direction;
				Freeze = previousState.Freeze;
			}
		}

		public abstract void HandleButton(InputButton button, InputButtonState state, Player0 player);

		/// <summary>
		/// base update, should be called from all inherited classes
		/// </summary>
		/// <param name="player"></param>
		/// <param name="environment"></param>
		public virtual void Update(Player0 player)
		{
			// update animation frame
			var current = GameLoop.GetElapsed();
			if (current - LastFrameChange > AnimationFrameDelay)
			{
				AnimationFrame++;
				LastFrameChange = current;
			}
		}
	}

	public class PlayerMovement : StateMachine<PlayerMovementState>
	{
		public new PlayerMovementState Pop()
		{
			return base.Pop() ?? new Idle();
		}
	}
}
