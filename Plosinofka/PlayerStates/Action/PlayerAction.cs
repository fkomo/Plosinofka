using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka
{
	public enum PlayerActionStateEnum
	{
		Idle,
		Shooting,
		Swinging,
	}

	public abstract class PlayerActionState : State<PlayerActionStateEnum>
	{
		public abstract void HandleButton(InputButton button, InputButtonState state, Player player);

		public abstract void Update(Player player, IRayCasting environment);
	}

	public class PlayerAction : StateMachine<PlayerActionState>
	{
		public new PlayerActionState Pop()
		{
			return base.Pop() ?? new IdleAction();
		}
	}
}
