using System.Threading;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game.Entities;
using Ujeby.Plosinofka.Game.Graphics;

namespace Ujeby.Plosinofka.Game.Core
{
	public class LoadingLevel : GameState
	{
		public override GameStateEnum AsEnum => GameStateEnum.LoadingLevel;

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
				// textures needs to be created on main thread
				SpriteCache.CreateTextures();

				// loading completed
				game.ChangeState(new Gameplay());
			}
		}

		public override void Render(Game0 game, double interpolation)
		{
			base.Render(game, interpolation);

			var view = new AABB(Vector2f.Zero, Vector2i.FullHD / 4); //game.Camera.InterpolatedView(interpolation);

			// progress bar
			Renderer.Instance.RenderRectangleOverlay(view,
				new AABB(new Vector2f(0, view.Size.Y - 4), new Vector2f(view.Size.X * (Progress / 100.0), view.Size.Y)), Color4b.White);

			//var buffer = new List<TextLine>()
			//{
			//	new Text { Value = $"Loading level '{ LevelName }' { string.Concat(Enumerable.Repeat(".", Progress)) }" },
			//};
			//var lines = buffer.ToArray();
			//Renderer.Instance.RenderTextLinesOverlay(view, new Vector2i(5, 5), lines, Color4b.White, 0.5);
		}

		private static void LoadLevel(string levelName, Game0 game)
		{
			game.ClearCurrentLevel();

			// load level
			var level = Level.Load(levelName);

			// make camera view smaller then window size for more pixelated look!
			var view = Vector2i.FullHD / 4;
			var cameraWindowSize = view / 3;
			var camera = new WindowCamera(view, new AABB(Vector2f.Zero, cameraWindowSize) + cameraWindowSize);

			game.SetCurrentLevel(level, camera, new Player0("ujeb")
			{
				Position = level.Start,
			});
		}
	}
}
