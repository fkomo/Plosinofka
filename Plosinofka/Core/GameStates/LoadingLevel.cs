using System.Collections.Generic;
using System.Threading;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game.Entities;
using Ujeby.Plosinofka.Game.Graphics;

namespace Ujeby.Plosinofka.Game.Core
{
	public class LoadingLevel : GameState
	{
		public override GameStateEnum AsEnum => GameStateEnum.Pause;

		private string LevelName;
		private Thread LoadingThread = null;

		public LoadingLevel(string levelName)
		{
			LevelName = levelName;
		}

		private int Progress = 0;

		public override void Update(Game0 game)
		{
			base.Update(game);

			if (LoadingThread == null)
			{
				Progress = 0;

				LoadingThread = new Thread(() => { LoadLevel(LevelName, game); });
				LoadingThread.Start();
			}
			else if (LoadingThread.IsAlive)
				Progress = (Progress + 1) % 100;

			else
			{
				LoadingThread.Join();
				LoadingThread = null;

				// textures needs to be created on main thread
				SpriteCache.CreateTextures();

				// loading completed
				game.ChangeState(new Gameplay());
			}
		}

		public override void Render(Game0 game, double interpolation)
		{
			base.Render(game, interpolation);

			var view = game.Camera.InterpolatedView(interpolation);

			var progressBarHeight = 4;

			// progress bar
			Renderer.Instance.RenderRectangleOverlay(view,
				new AABB(new Vector2f(0, view.Size.Y - progressBarHeight), new Vector2f(view.Size.X * (Progress / 100.0), view.Size.Y)), Color4b.White);

			var buffer = new List<TextLine>()
			{
				new Text { Value = $"{ LevelName }" },
			};
			var lines = buffer.ToArray();

			var font = Renderer.Instance.GetCurrentFont();
			Renderer.Instance.RenderTextLinesOverlay(view, new Vector2i(5, (int)view.Size.Y - 5 - progressBarHeight - font.CharSize.Y), lines, 1);
		}

		private static void LoadLevel(string levelName, Game0 game)
		{
			game.ClearCurrentLevel();

			// load level
			var level = Level.Load(levelName);

			game.SetCurrentLevel(
				level, 
				new Player0("ujeb")
				{
					Position = level.Start,
				});
		}
	}
}
