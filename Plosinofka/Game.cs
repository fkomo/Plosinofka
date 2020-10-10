using SDL2;
using System.Diagnostics;
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka
{
	class Game
	{
		public Simulation Simulation { get; private set; }
		public Input Input { get; private set; }

		private static readonly Stopwatch Stopwatch = new Stopwatch();

		public string Title { get; private set; }

		public Game(string title)
		{
			Initialize(title);
		}

		private void Initialize(string title)
		{
			Title = title;

			Input = new Input();
			
			Renderer.Instance.Initialize(new Vector2i(1920, 1080) / 2);

			Simulation = new Simulation();
		}

		public void Run()
		{
			var skipTicks = 1000 / Simulation.GameSpeed;

			var running = true;
			Stopwatch.Restart();

			var lastFrameTime = GetElapsed();
			var nextGameTick = lastFrameTime;
			while (running)
			{
				running = Input.Handle(Simulation);

				var loops = 0;
				while (GetElapsed() > nextGameTick && loops < Renderer.MaxFrameSkip)
				{
					Simulation.Update();

					nextGameTick += skipTicks;
					loops++;
				}

				var interpolation = (GetElapsed() + skipTicks - nextGameTick) / skipTicks;
				Renderer.Instance.Render(Simulation, interpolation);

				var fps = (int)(1000.0 / (GetElapsed() - lastFrameTime));
				lastFrameTime = GetElapsed();

				var title = $"{ Title } [fps: { fps } | lastUpdate: { Simulation.LastUpdateDuration:0.00}ms | lastRender: { Renderer.Instance.LastFrameDuration:0.00}ms]";
				Renderer.Instance.SetWindowTitle(title);
			}

			Simulation.Destroy();
			Renderer.Destroy();
			SDL.SDL_Quit();
		}

		/// <summary>
		/// return miliseconds of specified timer
		/// </summary>
		/// <returns></returns>
		public static double GetElapsed(Stopwatch stopwatch)
		{
			return stopwatch.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L)) / 1000.0;
		}

		/// <summary>
		/// return miliseconds since game loop start
		/// </summary>
		/// <returns></returns>
		public static double GetElapsed()
		{
			return GetElapsed(Stopwatch);
		}
	}
}
