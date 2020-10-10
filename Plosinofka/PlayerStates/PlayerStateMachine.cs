using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Common;

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
		HitWall,
	}

	internal abstract class PlayerState : State
	{
		public abstract void HandleButton(InputButton button, InputButtonState state, Player player);

		public abstract void Update(Player player);

		public override string ToString()
		{
			return AsEnum.ToString();
		}

		public abstract PlayerStateEnum AsEnum { get; }
	}

	class PlayerStateMachine : StateMachine<PlayerState>
	{
		public new PlayerState Pop()
		{
			return base.Pop() ?? new Standing();
		}
	}
}
