using SDL2;
using System;
using System.Collections.Generic;
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

		public bool Allocated() => Data != null && UnmanagedPtr != IntPtr.Zero && SdlSurfacePtr != IntPtr.Zero;
	}

	internal class Renderer : Singleton<Renderer>
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

		private Font CurrentFont;

		private ScreenBuffer ScreenBuffer;

		public Renderer()
		{
		}

		private ScreenBuffer CreateScreenBuffer(Vector2i size)
		{
			if (size.Area() * 4 != ScreenBuffer.Data?.Length)
			{ 
				// different size
				if (ScreenBuffer.Allocated())
				{
					// free old buffer
					SDL.SDL_FreeSurface(ScreenBuffer.SdlSurfacePtr);
					Marshal.FreeHGlobal(ScreenBuffer.UnmanagedPtr);
				}

				// create new buffer
				ScreenBuffer = new ScreenBuffer
				{
					Data = new byte[size.Area() * 4]
				};
				ScreenBuffer.UnmanagedPtr = Marshal.AllocHGlobal(ScreenBuffer.Data.Length);
				ScreenBuffer.SdlSurfacePtr = SDL.SDL_CreateRGBSurfaceFrom(ScreenBuffer.UnmanagedPtr,
					size.X, size.Y, 32, 4 * size.X,
					0xff000000,
					0x00ff0000,
					0x0000ff00,
					0x000000ff);

				Log.Add($"Renderer.ScreenBuffer({ size })");
			}

			return ScreenBuffer;
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

			SDL.SDL_SetRenderDrawBlendMode(RendererPtr, SDL.SDL_BlendMode.SDL_BLENDMODE_ADD);

			// TODO better font sprite (3x5 for character is not enough for lowercase)
			CurrentFont = new Font
			{
				SpriteId = SpriteCache.LoadSprite($".\\Content\\font-small.png")?.Id,
				CharSize = new Vector2i(3, 5),
				Spacing = new Vector2f(1, 2),
			};
		}

		internal static void Destroy()
		{
			if (instance.ScreenBuffer.Allocated())
			{
				SDL.SDL_FreeSurface(instance.ScreenBuffer.SdlSurfacePtr);
				Marshal.FreeHGlobal(instance.ScreenBuffer.UnmanagedPtr);
			}

			SDL.SDL_DestroyRenderer(instance.RendererPtr);
			SDL.SDL_DestroyWindow(instance.WindowPtr);
			instance = null;
		}

		public void Render(Simulation simulation, double interpolation)
		{
			var start = Game.GetElapsed();

			// clear backbuffer
			SDL.SDL_SetRenderDrawColor(RendererPtr, 0x0, 0x0, 0x0, 0xff);
			SDL.SDL_RenderClear(RendererPtr);

			simulation.Render(interpolation);

			// gui text
			RenderText(simulation.Camera, interpolation, new Vector2i(0, 0),
				"01234567890 ABCDEFGHIJKLMONOPRSTUVWXYZ abcdefghijklmnopqrstuvwxyz +-*= []{}<>\\/'\".:,;?|_");

			// display backbuffer
			SDL.SDL_RenderPresent(RendererPtr);

			FrameCount++;
			LastFrameDuration = Game.GetElapsed() - start;
		}

		internal void SetWindowTitle(string title)
		{
			SDL.SDL_SetWindowTitle(WindowPtr, title);
		}

		/// <summary>
		/// renders sprite layer on whole screen (camera view) with specified offset
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="sprite"></param>
		/// <param name="offset"></param>
		/// <param name="interpolation"></param>
		internal void RenderLayer(Camera camera, Sprite sprite, Vector2f offset)
		{
			var sourceRect = new SDL.SDL_Rect
			{
				x = (int)((sprite.Size.X - camera.View.X) * offset.X),
				y = (int)(sprite.Size.Y - camera.View.Y - ((sprite.Size.Y - camera.View.Y) * offset.Y)),
				w = camera.View.X,
				h = camera.View.Y
			};
			SDL.SDL_RenderCopy(RendererPtr, sprite.TexturePtr, ref sourceRect, IntPtr.Zero);
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
		internal void RenderLayer(Camera camera, double interpolation, Sprite colorLayer, Sprite dataLayer,
			Light[] lights, AABB[] occluders)
		{
			var cameraPosition = camera.InterpolatedPosition(interpolation);

			// draw color layer
			var sourceRect = new SDL.SDL_Rect
			{
				x = cameraPosition.X,
				y = colorLayer.Size.Y - camera.View.Y - cameraPosition.Y, // because sdl surface starts at topleft
				w = camera.View.X,
				h = camera.View.Y
			};
			SDL.SDL_RenderCopy(RendererPtr, colorLayer.TexturePtr, ref sourceRect, IntPtr.Zero);

			var shadingStart = Game.GetElapsed();
			if (dataLayer != null && Settings.Current.GetVisual(VisualSetting.Shading))
			{
				var screenBuffer = CreateScreenBuffer(camera.View);

				// shading from dynamic lights
				//for (var i = 0; i < ScreenBuffer.Data.Length / 4; i++)
				Parallel.For(0, screenBuffer.Data.Length / 4, (i, loopState) =>
				{
					var screen = new Vector2i(i % camera.View.X, i / camera.View.X);
					var wIndex = (cameraPosition.Y + screen.Y) * dataLayer.Size.X + cameraPosition.X + screen.X;
					var sIndex = ((camera.View.Y - screen.Y - 1) * camera.View.X + screen.X) * 4;

					if ((dataLayer.Data[wIndex] & Level.ShadeMask) == Level.ShadeMask)
					{
						var tmpColor = new Color4f(colorLayer.Data[wIndex]);
						if (tmpColor.A > 0)
						{
							tmpColor += Shading(screen + cameraPosition, lights, occluders) * 0.5;
							//tmpColor = tmpColor.GammaCorrection();
						}

						var finalColor = new Color4b(tmpColor);
						screenBuffer.Data[sIndex + 0] = finalColor.A;
						screenBuffer.Data[sIndex + 1] = finalColor.B;
						screenBuffer.Data[sIndex + 2] = finalColor.G;
						screenBuffer.Data[sIndex + 3] = finalColor.R;
					}
					else
						// just alpha channel
						screenBuffer.Data[sIndex] = 0;
				});

				// copy to data to unmanaged array
				Marshal.Copy(screenBuffer.Data, 0, screenBuffer.UnmanagedPtr,
					screenBuffer.Data.Length);

				// create texture
				var texturePtr = SDL.SDL_CreateTextureFromSurface(Instance.RendererPtr,
					screenBuffer.SdlSurfacePtr);

				// draw shading layer
				SDL.SDL_RenderCopy(RendererPtr, texturePtr, IntPtr.Zero, IntPtr.Zero);
				SDL.SDL_DestroyTexture(texturePtr);
			}

			LastShadingDuration = Game.GetElapsed() - shadingStart;
		}

		/// <summary>
		/// render color filled aabb
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="rectangle"></param>
		/// <param name="color"></param>
		/// <param name="interpolation"></param>
		public void RenderRectangle(Camera camera, AABB rectangle, Color4b color, double interpolation)
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
			SDL.SDL_SetRenderDrawColor(RendererPtr, color.R, color.G, color.B, color.A);
			SDL.SDL_RenderFillRect(RendererPtr, ref rect);
		}

		/// <summary>
		/// render colored line between point a and b
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="color"></param>
		/// <param name="interpolation"></param>
		public void RenderLine(Camera camera, Vector2f a, Vector2f b, Color4b color, double interpolation)
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
		/// <param name="sprite">width == height, if not sprite is considered as animation strip</param>
		/// <param name="interpolation"></param>
		public void RenderSprite(Camera camera, double interpolation, Vector2f spritePosition,
			Sprite sprite, int frame = 0)
		{
			if (sprite == null)
				return;

			var viewScale = CurrentWindowSize / camera.InterpolatedView(interpolation);
			var screenPosition = camera.RelateTo(spritePosition, interpolation) * viewScale;
			var spriteSize = sprite.Size.Y;

			var sourceRect = new SDL.SDL_Rect
			{
				x = spriteSize * frame,
				y = 0,
				w = spriteSize,
				h = spriteSize
			};
			var destinationRect = new SDL.SDL_Rect
			{
				x = (int)screenPosition.X,
				y = (int)((camera.View.Y - spriteSize) * viewScale.Y - screenPosition.Y),
				w = (int)(spriteSize * viewScale.X),
				h = (int)(spriteSize * viewScale.Y)
			};
			SDL.SDL_RenderCopy(RendererPtr, sprite.TexturePtr, ref sourceRect, ref destinationRect);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="interpolation"></param>
		/// <param name="screenPosition">topLeft corner (increasing from top to bottom)</param>
		/// <param name="text"></param>
		public void RenderText(Camera camera, double interpolation, Vector2i screenPosition, string text)
		{
			// TODO render font with variable character width
			var font = CurrentFont;
			var scale = CurrentWindowSize / camera.InterpolatedView(interpolation);
			var fontSprite = SpriteCache.Get(font.SpriteId);

			var sourceRect = new SDL.SDL_Rect();
			var destinationRect = new SDL.SDL_Rect();

			for (var i = 0; i < text.Length; i++)
			{
				var charIndex = (int)text[i] - 32;

				sourceRect.x = font.CharSize.X * charIndex;
				sourceRect.y = 0;
				sourceRect.w = font.CharSize.X;
				sourceRect.h = font.CharSize.Y;

				destinationRect.x = (int)(screenPosition.X + i * (font.CharSize.X + font.Spacing.X) * scale.X);
				destinationRect.y = (int)(screenPosition.Y * scale.Y);
				destinationRect.w = (int)(font.CharSize.X * scale.X);
				destinationRect.h = (int)(font.CharSize.Y * scale.Y);

				SDL.SDL_RenderCopy(RendererPtr, fontSprite.TexturePtr, ref sourceRect, ref destinationRect);
			}
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
