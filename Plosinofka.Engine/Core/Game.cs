using System;
using System.Diagnostics;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Graphics;

namespace Ujeby.Plosinofka.Engine.Core
{
	public class Game
	{
		private static readonly Stopwatch Stopwatch = new Stopwatch();

		private readonly string Title;

		public Game(string title, Renderer renderer, Simulation simulation, Input input)
		{
			Stopwatch.Restart();

			Title = title;
			Renderer.Instance = renderer;
			Simulation.Instance = simulation;
			Input.Instance = input;
		}

		public static int Fps = 0;

		public void Run()
		{
			Renderer.Instance.Initialize();
			Simulation.Instance.Initialize();

			Renderer.Instance.SetWindowTitle(Title);

			var skipTicks = 1000 / Simulation.Instance.GameSpeed;

			var running = true;

			var lastFrameTime = GetElapsed();
			var nextGameTick = lastFrameTime;

			var maxFps = double.NegativeInfinity;
			var minFps = double.PositiveInfinity;

			var loopStart = GetElapsed();
			while (running)
			{
				running = Input.Instance.Handle(Simulation.Instance);
				//simulation.Update();
				//Renderer.Instance.Render(simulation, 1.0);

				var loops = 0;
				while (GetElapsed() > nextGameTick && loops < Renderer.Instance.MaxFrameSkip)
				{
					Simulation.Instance.Update();
					nextGameTick += skipTicks;
					loops++;
				}
				var interpolation = (GetElapsed() + skipTicks - nextGameTick) / skipTicks;
				interpolation = Math.Clamp(interpolation, 0, 1);

				Renderer.Instance.Render(Simulation.Instance, interpolation);

				Fps = (int)(1000.0 / (GetElapsed() - lastFrameTime));

				if (Fps > maxFps)
					maxFps = Fps;
				if (Fps < minFps)
					minFps = Fps;

				lastFrameTime = GetElapsed();
			}

			var loopDuration = (GetElapsed() - loopStart);
			var avgFps = (int)(1000.0 / (loopDuration / Renderer.Instance.FramesRendered));
			Log.Add($"Game.Run(): gameLoop={ (int)(loopDuration / 1000)}s frames={ Renderer.Instance.FramesRendered }; avgFps={ avgFps }; minFps={ (int)minFps }; maxFps={ (int)maxFps };");
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
