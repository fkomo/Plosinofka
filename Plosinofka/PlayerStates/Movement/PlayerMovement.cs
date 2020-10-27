using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka
{
	public enum PlayerMovementStateEnum
	{
		Idle,
		Crouching,
		Walking,
		Running,
		Sneaking,
		Jumping,
		Falling,
		Dashing
	}

	public abstract class PlayerMovementState : State<PlayerMovementStateEnum>
	{
		public Vector2f Direction { get; protected set; }
		public bool Freeze { get; protected set; } = false;

		protected PlayerMovementState()
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

		public abstract void HandleButton(InputButton button, InputButtonState state, Player player);

		public abstract void Update(Player player, IRayCasting environment);
	}

	public class PlayerMovement : StateMachine<PlayerMovementState>
	{
		public new PlayerMovementState Pop()
		{
			return base.Pop() ?? new Idle();
		}
	}
}
