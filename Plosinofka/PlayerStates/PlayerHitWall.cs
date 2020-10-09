using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	class PlayerHitWall : PlayerState
	{
		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.HitWall; } }

		public PlayerHitWall()
		{

		}

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{

		}

		public override void Update(Player player)
		{
			player.CurrentState = PlayerStateMachine.Change(this, new PlayerStanding());
		}
	}
}
