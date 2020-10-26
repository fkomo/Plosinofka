using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	class IdleAction : PlayerActionState
	{
		public override PlayerActionStateEnum AsEnum { get { return PlayerActionStateEnum.Idle; } }

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{

		}

		public override void Update(Player player, IRayCasting environment)
		{

		}
	}
}
