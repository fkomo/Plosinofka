using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	class HitWall : PlayerState
	{
		public override PlayerStateEnum AsEnum { get { return PlayerStateEnum.HitWall; } }

		public HitWall()
		{

		}

		public override void HandleButton(InputButton button, InputButtonState state, Player player)
		{

		}

		public override void Update(Player player)
		{
			player.CurrentState = player.State.Change(this, new Standing());
		}
	}
}
