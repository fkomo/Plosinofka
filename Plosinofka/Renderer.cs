using SDL2;
using System;
using System.Diagnostics;
using System.Linq;
using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	class Renderer
	{
		private IntPtr WindowPtr { get; set; }
		public IntPtr RendererPtr { get; set; }

		public Vector2i WindowSize { get; private set; }

		/// <summary>max number of skippable frames to render</summary>
		public const int MaxFrameSkip = 5;
		public double LastFrameDuration { get; internal set; }

		private Stopwatch Stopwatch = new Stopwatch();

		public long FrameCount { get; private set; } = 0;

		private static Renderer instance = new Renderer();
		public static Renderer Instance 
		{ 
			get 
			{
				if (instance == null)
					instance = new Renderer();

				return instance; 
			} 
		}

		private Renderer()
		{

		}

		public void Initialize(Vector2i windowSize)
		{
			WindowSize = windowSize;

			if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
				throw new Exception($"Failed to initialize SDL2 library. SDL2Error({ SDL.SDL_GetError() })");

			WindowPtr = SDL.SDL_CreateWindow("sdl .net core test",
				SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED,
				WindowSize.X, WindowSize.Y,
				SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN);
			if (WindowPtr == null)
				throw new Exception($"Failed to create window. SDL2Error({ SDL.SDL_GetError() })");

			RendererPtr = SDL.SDL_CreateRenderer(WindowPtr, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
			if (RendererPtr == null)
				throw new Exception($"Failed to create renderer. SDL2Error({ SDL.SDL_GetError() })");
		}

		public void Render(Simulation simulation, double interpolation)
		{
			Stopwatch.Restart();

			// clear backbuffer
			SDL.SDL_SetRenderDrawColor(RendererPtr, 0, 0, 0, 255);
			SDL.SDL_RenderClear(RendererPtr);

			// render static world
			simulation.World.Render(simulation.Camera, null, interpolation);

			// render dynamic entities
			foreach (IRender entity in simulation.Entities.Where(e => e is IRender))
				entity.Render(simulation.Camera,
					simulation.EntitiesBeforeUpdate.SingleOrDefault(e => e.Id == (entity as Entity).Id), interpolation);

			// display backbuffer
			SDL.SDL_RenderPresent(RendererPtr);

			FrameCount++;
			Stopwatch.Stop();
			LastFrameDuration = Game.GetElapsed(Stopwatch);
		}

		internal void SetWindowTitle(string title)
		{
			SDL.SDL_SetWindowTitle(WindowPtr, title);
		}

		internal static void Destroy()
		{
			SDL.SDL_DestroyRenderer(instance.RendererPtr);
			SDL.SDL_DestroyWindow(instance.WindowPtr);
			instance = null;
		}
	}
}
