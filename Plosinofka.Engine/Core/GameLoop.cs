using System;
using System.Diagnostics;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Engine.Network;
using Ujeby.Plosinofka.Engine.Network.Messages;

namespace Ujeby.Plosinofka.Engine.Core
{
	public class GameLoop
	{
		private static readonly Stopwatch Stopwatch = new Stopwatch();

		private readonly string Title;

		public GameLoop(string title, Renderer renderer, Game game, Input input)
		{
			Stopwatch.Restart();

			Title = title;
			Renderer.Instance = renderer;
			Game.Instance = game;
			Input.Instance = input;
		}

		public static int Fps = 0;

		public void Run()
		{
			Renderer.Instance.Initialize();
			Renderer.Instance.SetWindowTitle(Title);
			
			Game.Instance.Initialize();

			var skipTicks = 1000 / Game.Instance.GameSpeed;

			var lastFrameTime = GetElapsed();
			var nextGameTick = lastFrameTime;

			var maxFps = double.NegativeInfinity;
			var minFps = double.PositiveInfinity;

			var loopStart = GetElapsed();
			while (Input.Instance.Handle(Game.Instance))
			{
				//simulation.Update();
				//Renderer.Instance.Render(simulation, 1.0);

				// update specified [Simulation.Instance.GameSpeed] number of times per second
				var loops = 0;
				while (GetElapsed() > nextGameTick && loops < Renderer.Instance.MaxFrameSkip)
				{
					Game.Instance.Update();
					nextGameTick += skipTicks;
					loops++;
				}
				var interpolation = (GetElapsed() + skipTicks - nextGameTick) / skipTicks;
				interpolation = Math.Clamp(interpolation, 0, 1);

				// render is called as much as possible with interpolation 0..1 (time between updates)
				Renderer.Instance.Render(Game.Instance, interpolation);

				// update fps for statistics
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
