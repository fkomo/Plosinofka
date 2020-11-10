using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Game.Entities;

namespace Ujeby.Plosinofka.Game.PlayerStates
{
	class IdleAction : PlayerActionState
	{
		public override PlayerActionStateEnum AsEnum { get { return PlayerActionStateEnum.Idle; } }

		public override void HandleButton(InputButton button, InputButtonState state, Player0 player)
		{

		}

		public override void Update(Player0 player)
		{

		}
	}
}
