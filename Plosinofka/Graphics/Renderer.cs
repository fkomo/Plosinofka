using SDL2;
using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;

namespace Ujeby.Plosinofka.Graphics
{
	class Renderer
	{
		private IntPtr WindowPtr { get; set; }
		public IntPtr RendererPtr { get; set; }

		public Vector2i WindowSize { get; private set; }

		/// <summary>max number of skippable frames to render</summary>
		public const int MaxFrameSkip = 5;
		public double LastFrameDuration { get; internal set; }

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
			var start = Game.GetElapsed();

			// clear backbuffer
			SDL.SDL_SetRenderDrawColor(RendererPtr, 0, 0, 0, 255);
			SDL.SDL_RenderClear(RendererPtr);

			simulation.Render(interpolation);

			// display backbuffer
			SDL.SDL_RenderPresent(RendererPtr);

			FrameCount++;
			LastFrameDuration = Game.GetElapsed() - start;
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

		/// <summary>
		/// render sprite to screen
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="worldPosition">sprite position in world (bottomLeft)</param>
		/// <param name="sprite"></param>
		/// <param name="interpolation"></param>
		internal void RenderSprite(Camera camera, Vector2f worldPosition, Sprite sprite, double interpolation)
		{
			var viewScale = WindowSize / camera.InterpolatedView(interpolation);
			var relativeToCamera = camera.RelateTo(worldPosition, interpolation) * viewScale;

			// SDL rect starts at topLeft
			var destination = new SDL.SDL_Rect
			{
				x = (int)relativeToCamera.X,
				y = (int)((camera.View.Y - sprite.Size.Y) * viewScale.Y - relativeToCamera.Y),
				w = (int)(sprite.Size.X * viewScale.X),
				h = (int)(sprite.Size.Y * viewScale.Y),
			};
			SDL.SDL_RenderCopy(RendererPtr, sprite.TexturePtr, IntPtr.Zero, ref destination);
		}

		/// <summary>
		/// render sprite to screen
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="sprite"></param>
		/// <param name="interpolation"></param>
		internal void RenderSprite(Camera camera, Sprite sprite, double interpolation)
		{
			var cameraPosition = camera.GetPosition(interpolation);

			var source = new SDL.SDL_Rect
			{
				x = (int)cameraPosition.X,
				y = (int)(sprite.Size.Y - camera.View.Y - cameraPosition.Y), // because sdl surface starts at topleft
				w = camera.View.X,
				h = camera.View.Y
			};
			SDL.SDL_RenderCopy(RendererPtr, sprite.TexturePtr, ref source, IntPtr.Zero);
		}
	}
}
