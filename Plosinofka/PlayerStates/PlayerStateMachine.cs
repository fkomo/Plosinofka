using System.Collections.Generic;
using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	enum PlayerStateEnum
	{
		Standing,
		Crouching,
		Walking,
		Running,
		Sneaking,
		Jumping,
	}

	internal abstract class PlayerState
	{
		public abstract void HandleButton(InputButton button, InputButtonState state, Player player);

		public abstract void Update(Player player);

		public abstract PlayerStateEnum AsEnum { get; }
	}

	class PlayerStateMachine
	{
		public static Stack<PlayerState> States = new Stack<PlayerState>();

		public static void Clear()
		{
			States.Clear();
		}

		public static PlayerState Push(PlayerState state)
		{
			States.Push(state);
			return state;
		}

		public static PlayerState Pop()
		{
			if (States.TryPop(out PlayerState lastState))
				return lastState;

			return new PlayerStanding();
		}

		public static PlayerState Change(PlayerState from, PlayerState to)
		{
			Log.Add($"PlayerStateChange({ from?.AsEnum } -> { to.AsEnum })");

			if (from != null)
				Push(from);

			return to;
		}
	}
}
