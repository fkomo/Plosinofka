using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game.Entities;

namespace Ujeby.Plosinofka.Game.PlayerStates
{
	public enum PlayerActionStateEnum
	{
		Idle,
		Shooting,
		Swinging,
	}

	public abstract class PlayerActionState : State<PlayerActionStateEnum>
	{
		public abstract void HandleButton(InputButton button, InputButtonState state, Player0 player);

		public abstract void Update(Player0 player, IRayCasting environment);
	}

	public class PlayerAction : StateMachine<PlayerActionState>
	{
		public new PlayerActionState Pop()
		{
			return base.Pop() ?? new IdleAction();
		}
	}
}
