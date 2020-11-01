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
		public long FramesRendered { get; private set; } = 0;

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
			RenderText(simulation.Camera.InterpolatedView(interpolation), new Vector2i(0, 0),
				"01234567890 ABCDEFGHIJKLMONOPRSTUVWXYZ abcdefghijklmnopqrstuvwxyz +-*= []{}<>\\/'\".:,;?|_");

			// display backbuffer
			SDL.SDL_RenderPresent(RendererPtr);

			FramesRendered++;
			LastFrameDuration = Game.GetElapsed() - start;
		}

		internal void SetWindowTitle(string title)
		{
			SDL.SDL_SetWindowTitle(WindowPtr, title);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="layer"></param>
		/// <param name="view"></param>
		internal void RenderLayer(AABB view, Layer layer)
		{
			var colorSprite = SpriteCache.Get(layer.ColorMapId);

			var sourceRect = new SDL.SDL_Rect
			{
				x = (int)view.Left,
				y = (int)(colorSprite.Size.Y - view.Top),
				w = (int)view.Size.X,
				h = (int)view.Size.Y
			};

			SDL.SDL_RenderCopy(RendererPtr, colorSprite.TexturePtr, ref sourceRect, IntPtr.Zero);
		}

		internal void RenderLayer(AABB view, Layer layer, Light[] lights, AABB[] obstacles)
		{
			var shadingStart = Game.GetElapsed();

			if (Settings.Current.GetVisual(VisualSetting.Shading) && layer.DataMapId != null)
			{
				var data = SpriteCache.Get(layer.DataMapId);
				var color = SpriteCache.Get(layer.ColorMapId);

				var screenBuffer = CreateScreenBuffer(view.Size);

				// shading from dynamic lights
				//for (var i = 0; i < ScreenBuffer.Data.Length / 4; i++)
				Parallel.For(0, screenBuffer.Data.Length / 4, (i, loopState) =>
				{
					var screen = new Vector2i(i % (int)view.Size.X, i / (int)view.Size.X);
					var wIndex = ((int)view.Bottom + screen.Y) * data.Size.X + (int)view.Left + screen.X;
					var sIndex = (((int)view.Size.Y - screen.Y - 1) * (int)view.Size.X + screen.X) * 4;

					if ((data.Data[wIndex] & Level.ShadeMask) == Level.ShadeMask)
					{
						var tmpColor = new Color4f(color.Data[wIndex]);
						if (tmpColor.A > 0)
						{
							tmpColor += RayTracer.Shade(screen + view.Min, lights, obstacles) * 0.5;
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
				}
				);

				// copy to data to unmanaged array
				Marshal.Copy(screenBuffer.Data, 0, screenBuffer.UnmanagedPtr, screenBuffer.Data.Length);

				// create texture
				var texture = SDL.SDL_CreateTextureFromSurface(Instance.RendererPtr, screenBuffer.SdlSurfacePtr);

				// draw shading layer
				SDL.SDL_RenderCopy(RendererPtr, texture, IntPtr.Zero, IntPtr.Zero);
				SDL.SDL_DestroyTexture(texture);
			}

			LastShadingDuration = Game.GetElapsed() - shadingStart;
		}

		/// <summary>
		/// render color filled aabb
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="rectangle"></param>
		/// <param name="color"></param>
		public void RenderRectangle(AABB view, AABB rectangle, Color4b color)
		{
			var scale = CurrentWindowSize / view.Size;
			var screenSpace = (rectangle.Min - view.Min) * scale;

			var rect = new SDL.SDL_Rect
			{
				x = (int)screenSpace.X,
				y = (int)((view.Size.Y - rectangle.Size.Y) * scale.Y - screenSpace.Y),
				w = (int)(rectangle.Size.X * scale.X),
				h = (int)(rectangle.Size.Y * scale.Y),
			};
			SDL.SDL_SetRenderDrawColor(RendererPtr, color.R, color.G, color.B, color.A);
			SDL.SDL_RenderFillRect(RendererPtr, ref rect);
		}

		/// <summary>
		/// render colored line between point a and b
		/// </summary>
		/// <param name="view"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="color"></param>
		public void RenderLine(AABB view, Vector2f a, Vector2f b, Color4b color)
		{
			var scale = CurrentWindowSize / view.Size;

			// line points in screen space
			a = (a - view.Min) * scale;
			b = (b - view.Min) * scale;

			SDL.SDL_SetRenderDrawColor(RendererPtr, color.R, color.G, color.B, color.A);
			SDL.SDL_RenderDrawLine(RendererPtr,
				(int)a.X, (int)(view.Size.Y * scale.Y - a.Y),
				(int)b.X, (int)(view.Size.Y * scale.Y - b.Y));
		}

		/// <summary>
		/// render sprite to screen
		/// </summary>
		/// <param name="view"></param>
		/// <param name="sprite">width == height, if not sprite is considered as animation strip</param>
		/// <param name="spritePosition">sprite position in world (bottomLeft)</param>
		public void RenderSprite(AABB view, Sprite sprite, Vector2f spritePosition, int frame = 0)
		{
			if (sprite == null)
				return;

			var scale = CurrentWindowSize / view.Size;
			var screenSpace = (spritePosition - view.Min) * scale;
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
				x = (int)screenSpace.X,
				y = (int)((view.Size.Y - spriteSize) * scale.Y - screenSpace.Y),
				w = (int)(spriteSize * scale.X),
				h = (int)(spriteSize * scale.Y)
			};
			SDL.SDL_RenderCopy(RendererPtr, sprite.TexturePtr, ref sourceRect, ref destinationRect);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="position">topLeft corner (increasing from top to bottom)</param>
		/// <param name="text"></param>
		public void RenderText(AABB view, Vector2i position, string text)
		{
			// TODO render font with variable character width
			var font = CurrentFont;
			var fontSprite = SpriteCache.Get(font.SpriteId);

			var scale = CurrentWindowSize / view.Size;

			var sourceRect = new SDL.SDL_Rect();
			var destinationRect = new SDL.SDL_Rect();

			for (var i = 0; i < text.Length; i++)
			{
				var charIndex = (int)text[i] - 32;

				sourceRect.x = font.CharSize.X * charIndex;
				sourceRect.y = 0;
				sourceRect.w = font.CharSize.X;
				sourceRect.h = font.CharSize.Y;

				destinationRect.x = (int)(position.X + i * (font.CharSize.X + font.Spacing.X) * scale.X);
				destinationRect.y = (int)(position.Y * scale.Y);
				destinationRect.w = (int)(font.CharSize.X * scale.X);
				destinationRect.h = (int)(font.CharSize.Y * scale.Y);

				SDL.SDL_RenderCopy(RendererPtr, fontSprite.TexturePtr, ref sourceRect, ref destinationRect);
			}
		}
	}
}
