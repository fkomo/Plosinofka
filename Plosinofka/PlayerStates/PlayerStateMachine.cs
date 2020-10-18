﻿using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka
{
	public enum PlayerStateEnum
	{
		Standing,
		Crouching,
		Walking,
		Running,
		Sneaking,
		Jumping,
		Falling,
	}

	public abstract class PlayerState : State
	{
		public abstract void HandleButton(InputButton button, InputButtonState state, Player player);

		public abstract void Update(Player player, IRayCasting environment);

		public override string ToString()
		{
			return AsEnum.ToString();
		}

		public abstract PlayerStateEnum AsEnum { get; }
	}

	public class PlayerStateMachine : StateMachine<PlayerState>
	{
		public new PlayerState Pop()
		{
			return base.Pop() ?? new Standing();
		}

		public PlayerState Change(PlayerState previous, PlayerState next, bool pushPreviousState = true)
		{
			if (previous != null && pushPreviousState)
				Push(previous);

			return next;
		}
	}
}
