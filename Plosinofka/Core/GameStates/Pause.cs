using System.Collections.Generic;
using System.Threading;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game.Entities;
using Ujeby.Plosinofka.Game.Graphics;

namespace Ujeby.Plosinofka.Game.Core
{
	public class Pause : GameState
	{
		public override GameStateEnum AsEnum => GameStateEnum.Pause;

		public Pause()
		{
		}

		public override void Update(Game0 game)
		{
			base.Update(game);


		}

		public override void Render(Game0 game, double interpolation)
		{
			base.Render(game, interpolation);

			var view = game.Camera.InterpolatedView(interpolation);


		}
	}
}
