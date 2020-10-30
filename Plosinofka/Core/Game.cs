using SDL2;
using System.Diagnostics;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Graphics;

namespace Ujeby.Plosinofka.Core
{
	class Game
	{
		private static readonly Stopwatch Stopwatch = new Stopwatch();
		private string Title;

		public Game(string title, Vector2i windowSize)
		{
			Title = title;
			Stopwatch.Restart();

			Renderer.Instance.Initialize(windowSize);
		}

		public static int Fps = 0;

		public void Run()
		{
			World.Instance.Load("world1");

			var skipTicks = 1000 / Simulation.GameSpeed;

			var running = true;

			var lastTitleUpdate = 0.0;
			var lastFrameTime = GetElapsed();
			var nextGameTick = lastFrameTime;

			var maxFps = double.NegativeInfinity;
			var minFps = double.PositiveInfinity;

			var loopStart = GetElapsed();
			while (running)
			{
				running = Input.Handle(World.Instance);
				//simulation.Update();
				//Renderer.Instance.Render(simulation, 1.0);

				var loops = 0;
				while (GetElapsed() > nextGameTick && loops < Renderer.MaxFrameSkip)
				{
					World.Instance.Update();
					nextGameTick += skipTicks;
					loops++;
				}
				var interpolation = (GetElapsed() + skipTicks - nextGameTick) / skipTicks;
				Renderer.Instance.Render(World.Instance, interpolation);

				Fps = (int)(1000.0 / (GetElapsed() - lastFrameTime));

				if (Fps > maxFps)
					maxFps = Fps;
				if (Fps < minFps)
					minFps = Fps;

				lastFrameTime = GetElapsed();

				if (lastFrameTime - lastTitleUpdate > 500)
				{
					var upd = $"{ World.Instance.LastUpdateDuration:0.00}";
					var render = $"{ Renderer.Instance.LastFrameDuration:0.00}";
					var shading = $"{ (int)(Renderer.Instance.LastShadingDuration / Renderer.Instance.LastFrameDuration * 100) }";

					var loopTime = (int)((GetElapsed() - loopStart) / 1000);
					var title = $"{ Title } [time: { loopTime }s | fps: { Fps } | update:{ upd }ms | render:{ render }ms ~(shading:{ shading }%) | ]";
					Renderer.Instance.SetWindowTitle(title);
					lastTitleUpdate = lastFrameTime;
				}
			}

			var loopDuration = (GetElapsed() - loopStart);
			var avgFps = (int)(1000.0 / (loopDuration / Renderer.Instance.FrameCount));
			Log.Add($"Game.Run(): gameLoop={ (int)(loopDuration / 1000)}s frames={ Renderer.Instance.FrameCount }; avgFps={ avgFps }; minFps={ (int)minFps }; maxFps={ (int)maxFps };");

			World.Destroy();
			Renderer.Destroy();
			SDL.SDL_Quit();
		}

		/// <summary>
		/// return miliseconds since game loop start
		/// </summary>
		/// <returns></returns>
		public static double GetElapsed()
		{
			return Stopwatch.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L)) / 1000.0;
		}
	}
}
