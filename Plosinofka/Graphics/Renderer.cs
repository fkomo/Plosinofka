using SDL2;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka.Graphics
{
	internal struct ScreenBuffer
	{
		public byte[] Data;
		public IntPtr SdlSurfacePtr;
		public IntPtr UnmanagedPtr;
	}

	internal class Renderer
	{
		public IntPtr WindowPtr { get; private set; }
		public IntPtr RendererPtr { get; private set; }

		public Vector2i CurrentWindowSize
		{
			get
			{
				SDL.SDL_GetWindowSize(WindowPtr, out int width, out int height);
				return new Vector2i(width, height);
			}
		}

		/// <summary>max number of skippable frames to render</summary>
		public const int MaxFrameSkip = 5;
		public double LastFrameDuration { get; internal set; }
		public double LastShadingDuration { get; internal set; }
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

		internal void SettingsChanged(VisualSetting setting)
		{
			Log.Add($"Renderer.SettingsChanged({ setting }): { Settings.Current.GetVisual(setting) }");
			// TODO do what needs to be done when visual setting was changed	
		}

		private ScreenBuffer ScreenBuffer;

		private Renderer()
		{
			var bufferSize = Vector2i.FullHD / 4;

			ScreenBuffer = new ScreenBuffer
			{
				Data = new byte[bufferSize.Area() * 4]
			};
			ScreenBuffer.UnmanagedPtr = Marshal.AllocHGlobal(ScreenBuffer.Data.Length);
			ScreenBuffer.SdlSurfacePtr = SDL.SDL_CreateRGBSurfaceFrom(ScreenBuffer.UnmanagedPtr,
				bufferSize.X, bufferSize.Y, 32, 4 * bufferSize.X,
				0xff000000,
				0x00ff0000,
				0x0000ff00,
				0x000000ff);
		}

		public void Initialize(Vector2i windowSize)
		{
			if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
				throw new Exception($"Failed to initialize SDL2 library. SDL2Error({ SDL.SDL_GetError() })");

			WindowPtr = SDL.SDL_CreateWindow("sdl .net core test",
				SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED,
				windowSize.X, windowSize.Y, SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL);
			if (WindowPtr == null)
				throw new Exception($"Failed to create window. SDL2Error({ SDL.SDL_GetError() })");

			RendererPtr = SDL.SDL_CreateRenderer(WindowPtr, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
			if (RendererPtr == null)
				throw new Exception($"Failed to create renderer. SDL2Error({ SDL.SDL_GetError() })");
		}

		internal static void Destroy()
		{
			SDL.SDL_FreeSurface(instance.ScreenBuffer.SdlSurfacePtr);
			Marshal.FreeHGlobal(instance.ScreenBuffer.UnmanagedPtr);

			SDL.SDL_DestroyRenderer(instance.RendererPtr);
			SDL.SDL_DestroyWindow(instance.WindowPtr);
			instance = null;
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

		private SDL.SDL_Rect RenderRect = new SDL.SDL_Rect();

		/// <summary>
		/// renders sprite layer on whole screen (camera view) with specified offset
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="layer"></param>
		/// <param name="layerOffset"></param>
		/// <param name="interpolation"></param>
		internal void RenderLayer(Camera camera, Sprite layer, Vector2f layerOffset)
		{
			RenderRect.x = (int)((layer.Size.X - camera.View.X) * layerOffset.X);
			RenderRect.y = (int)(layer.Size.Y - camera.View.Y - ((layer.Size.Y - camera.View.Y) * layerOffset.Y));
			RenderRect.w = camera.View.X;
			RenderRect.h = camera.View.Y;
			SDL.SDL_RenderCopy(RendererPtr, layer.TexturePtr, ref RenderRect, IntPtr.Zero);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="color"></param>
		/// <param name="dataLayer"></param>
		/// <param name="interpolation"></param>
		/// <param name="lights"></param>
		/// <param name="occluders"></param>
		internal void Render(Camera camera, Sprite colorLayer, Sprite dataLayer, double interpolation, 
			Light[] lights, AABB[] occluders)
		{
			var cameraPosition = camera.InterpolatedPosition(interpolation);

			// draw color layer
			RenderRect.x = cameraPosition.X;
			RenderRect.y = colorLayer.Size.Y - camera.View.Y - cameraPosition.Y; // because sdl surface starts at topleft
			RenderRect.w = camera.View.X;
			RenderRect.h = camera.View.Y;
			SDL.SDL_RenderCopy(RendererPtr, colorLayer.TexturePtr, ref RenderRect, IntPtr.Zero);

			var shadingStart = Game.GetElapsed();
			if (dataLayer != null && Settings.Current.GetVisual(VisualSetting.Shading))
			{
				// shading from dynamic lights
				//for (var i = 0; i < ScreenBuffer.Data.Length / 4; i++)
				Parallel.For(0, ScreenBuffer.Data.Length / 4, (i, loopState) =>
				{
					var screen = new Vector2i(i % camera.View.X, i / camera.View.X);
					var worldMapIndex = (cameraPosition.Y + screen.Y) * dataLayer.Size.X + cameraPosition.X + screen.X;
					var screenIndex = ((camera.View.Y - screen.Y - 1) * camera.View.X + screen.X) * 4;

 					if (Level.IsShadowReceiver(dataLayer.Data[worldMapIndex]))
					{
						var tmpColor = new Color4f(colorLayer.Data[worldMapIndex]);
						tmpColor += Shading(screen + cameraPosition, lights, occluders) * 0.5;
						//tmpColor = tmpColor.GammaCorrection();

						var finalColor = new Color4b(tmpColor);
						ScreenBuffer.Data[screenIndex + 0] = finalColor.A;
						ScreenBuffer.Data[screenIndex + 1] = finalColor.B;
						ScreenBuffer.Data[screenIndex + 2] = finalColor.G;
						ScreenBuffer.Data[screenIndex + 3] = finalColor.R;
					}
					else
						// just alpha channel
						ScreenBuffer.Data[screenIndex] = 0;
				});

				// copy to data to unmanaged array
				Marshal.Copy(ScreenBuffer.Data, 0, ScreenBuffer.UnmanagedPtr,
					ScreenBuffer.Data.Length);

				// create texture
				var texturePtr = SDL.SDL_CreateTextureFromSurface(Instance.RendererPtr,
					ScreenBuffer.SdlSurfacePtr);

				// draw shading layer
				SDL.SDL_RenderCopy(RendererPtr, texturePtr, IntPtr.Zero, IntPtr.Zero);
				SDL.SDL_DestroyTexture(texturePtr);
			}
			LastShadingDuration = Game.GetElapsed() - shadingStart;
		}

		internal void RenderRectangle(Camera camera, AABB rectangle, Color4f color, double interpolation)
		{
			var viewScale = CurrentWindowSize / camera.InterpolatedView(interpolation);
			var relativeToCamera = camera.RelateTo(rectangle.Min, interpolation) * viewScale;

			var rect = new SDL.SDL_Rect
			{
				x = (int)relativeToCamera.X,
				y = (int)((camera.View.Y - rectangle.Size.Y) * viewScale.Y - relativeToCamera.Y),
				w = (int)(rectangle.Size.X * viewScale.X),
				h = (int)(rectangle.Size.Y * viewScale.Y),
			};

			var _color = new Color4b(color);
			SDL.SDL_SetRenderDrawColor(RendererPtr, _color.R, _color.G, _color.B, _color.A);
			SDL.SDL_RenderFillRect(RendererPtr, ref rect);
		}

		internal void RenderLine(Camera camera, Vector2f a, Vector2f b, Color4b color, double interpolation)
		{
			var viewScale = CurrentWindowSize / camera.InterpolatedView(interpolation);
			a = camera.RelateTo(a, interpolation) * viewScale;
			b = camera.RelateTo(b, interpolation) * viewScale;

			SDL.SDL_SetRenderDrawColor(RendererPtr, color.R, color.G, color.B, color.A);
			SDL.SDL_RenderDrawLine(RendererPtr,
				(int)a.X, (int)(camera.View.Y * viewScale.Y - a.Y),
				(int)b.X, (int)(camera.View.Y * viewScale.Y - b.Y));
		}

		/// <summary>
		/// render sprite to screen
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="spritePosition">sprite position in world (bottomLeft)</param>
		/// <param name="sprite"></param>
		/// <param name="interpolation"></param>
		internal void RenderSprite(Camera camera, Sprite sprite, Vector2f spritePosition, double interpolation)
		{
			var viewScale = CurrentWindowSize / camera.InterpolatedView(interpolation);
			var screenPosition = camera.RelateTo(spritePosition, interpolation) * viewScale;

			RenderRect.x = (int)screenPosition.X;
			RenderRect.y = (int)((camera.View.Y - sprite.Size.Y) * viewScale.Y - screenPosition.Y);
			RenderRect.w = (int)(sprite.Size.X * viewScale.X);
			RenderRect.h = (int)(sprite.Size.Y * viewScale.Y);
			SDL.SDL_RenderCopy(RendererPtr, sprite.TexturePtr, IntPtr.Zero, ref RenderRect);
		}

		private Color4f Shading(Vector2i pixel, Light[] lights, AABB[] occluders)
		{
			var origin = (Vector2f)pixel;
			var result = Color4f.Black;
			foreach (var light in lights)
			{
				var lightDistance = (light.Position - origin).Length();
				var ray = new Ray(origin, light.Position - origin);

				var occluded = false;
				foreach (var occluder in occluders)
				{
					if (occluder.Intersects(ray, to: lightDistance))
					{
						occluded = true;
						break;
					}
				}
				if (occluded)
					continue;

				result += light.Color * (light.Intensity / (lightDistance));
			}

			return result;
		}
	}
}
