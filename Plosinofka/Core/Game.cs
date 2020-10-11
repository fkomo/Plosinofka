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

		public Input Input { get; private set; }

		public Game(string title, Vector2i windowSize)
		{
			Title = title;
			Stopwatch.Restart();

			Renderer.Instance.Initialize(windowSize);
		}

		public void Run()
		{
			var simulation = new World();

			var skipTicks = 1000 / Simulation.GameSpeed;

			var running = true;

			var lastFrameTime = GetElapsed();
			var nextGameTick = lastFrameTime;
			while (running)
			{
				running = Input.Handle(simulation);

				var loops = 0;
				while (GetElapsed() > nextGameTick && loops < Renderer.MaxFrameSkip)
				{
					simulation.Update();

					nextGameTick += skipTicks;
					loops++;
				}

				var interpolation = (GetElapsed() + skipTicks - nextGameTick) / skipTicks;
				Renderer.Instance.Render(simulation, interpolation);

				var fps = (int)(1000.0 / (GetElapsed() - lastFrameTime));
				lastFrameTime = GetElapsed();

				var title = $"{ Title } [fps: { fps } | lastUpdate: { simulation.LastUpdateDuration:0.00}ms | lastRender: { Renderer.Instance.LastFrameDuration:0.00}ms]";
				Renderer.Instance.SetWindowTitle(title);
			}

			simulation.Destroy();
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
