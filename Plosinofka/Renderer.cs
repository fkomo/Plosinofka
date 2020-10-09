using SDL2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	class Renderer
	{
		private IntPtr SDLWindow { get; set; }
		private IntPtr SDLRenderer { get; set; }
		
		/// <summary>
		/// max number of skippable frames to render
		/// </summary>
		public int MaxFrameSkip { get; internal set; } = 5;
		public double LastFrameDuration { get; internal set; }

		private Stopwatch Stopwatch = new Stopwatch();

		public long FrameCount { get; private set; } = 0;

		public Renderer()
		{
			Initialize();
		}

		private void Initialize()
		{
			if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
				throw new Exception($"Failed to initialize SDL2 library. SDL2Error({ SDL.SDL_GetError() })");

			SDLWindow = SDL.SDL_CreateWindow("sdl .net core test",
				SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED,
				1920 / 2, 1080 / 2,
				SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN);
			if (SDLWindow == null)
				throw new Exception($"Failed to create window. SDL2Error({ SDL.SDL_GetError() })");

			SDLRenderer = SDL.SDL_CreateRenderer(SDLWindow, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
			if (SDLRenderer == null)
				throw new Exception($"Failed to create renderer. SDL2Error({ SDL.SDL_GetError() })");
		}

		public void Render(Simulation simulation, double interpolation)
		{
			Stopwatch.Restart();

			// clear backbuffer
			SDL.SDL_SetRenderDrawColor(SDLRenderer, 0, 0, 0, 255);
			SDL.SDL_RenderClear(SDLRenderer);

			// render static world
			simulation.World.Render(SDLRenderer, null, interpolation);

			// render dynamic entities
			foreach (IRender entity in simulation.Entities.Where(e => e is IRender))
				entity.Render(SDLRenderer,
					simulation.EntitiesBeforeUpdate.SingleOrDefault(e => e.Id == (entity as Entity).Id), interpolation);

			// display backbuffer
			SDL.SDL_RenderPresent(SDLRenderer);

			FrameCount++;
			Stopwatch.Stop();
			LastFrameDuration = Game.GetElapsed(Stopwatch);
		}

		internal void SetWindowTitle(string title)
		{
			SDL.SDL_SetWindowTitle(SDLWindow, title);
		}

		internal void Destroy()
		{
			SDL.SDL_DestroyRenderer(SDLRenderer);
			SDL.SDL_DestroyWindow(SDLWindow);
		}
	}
}
