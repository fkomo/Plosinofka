using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Entities;
using Ujeby.Plosinofka.Engine.Graphics;

namespace Ujeby.Plosinofka.Engine.SDL
{
	public enum BufferEnum : int
	{
		PerPixelShading = 0,
		AmbientOcclusion1,
		AmbientOcclusion2,
		DirectIllumination,
	}

	public struct ScreenBuffer
	{
		public byte[] Data;
		public IntPtr SdlSurfacePtr;
		public IntPtr UnmanagedPtr;

		public bool Allocated() => Data != null && UnmanagedPtr != IntPtr.Zero && SdlSurfacePtr != IntPtr.Zero;
	}

	public class SDL2Renderer : Renderer
	{
		public IntPtr WindowPtr { get; private set; }
		public IntPtr RendererPtr { get; private set; }

		public override Vector2i CurrentWindowSize
		{
			get
			{
				SDL2.SDL.SDL_GetWindowSize(WindowPtr, out int width, out int height);
				return new Vector2i(width, height);
			}
		}

		/// <summary>max number of skippable frames to render</summary>
		public override int MaxFrameSkip { get; protected set; } = 5;
		public override double LastFrameDuration { get; protected set; }
		public override long FramesRendered { get; protected set; } = 0;

		private Font CurrentFont;

		public override Font GetCurrentFont() => CurrentFont;

		public readonly Dictionary<BufferEnum, ScreenBuffer> ScreenBuffers = new Dictionary<BufferEnum, ScreenBuffer>();
		public ScreenBuffer CreateScreenBuffer(BufferEnum id, Vector2i size)
		{
			var bufferFound = ScreenBuffers.TryGetValue(id, out ScreenBuffer buffer);

			if (!bufferFound || size.Area() * 4 != buffer.Data?.Length)
			{
				// different size
				if (bufferFound && buffer.Allocated())
				{
					// free old buffer
					SDL2.SDL.SDL_FreeSurface(buffer.SdlSurfacePtr);
					Marshal.FreeHGlobal(buffer.UnmanagedPtr);
				}

				// create new buffer
				buffer = new ScreenBuffer
				{
					Data = new byte[size.Area() * 4]
				};
				buffer.UnmanagedPtr = Marshal.AllocHGlobal(buffer.Data.Length);
				buffer.SdlSurfacePtr = SDL2.SDL.SDL_CreateRGBSurfaceFrom(buffer.UnmanagedPtr,
					size.X, size.Y, 32, 4 * size.X,
					0xff000000,
					0x00ff0000,
					0x0000ff00,
					0x000000ff);

				if (!bufferFound)
					ScreenBuffers.Add(id, buffer);
				else
					ScreenBuffers[id] = buffer;

				Log.Add($"Renderer.ScreenBuffer({ id }, { size })");
			}

			return buffer;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="windowSize"></param>
		/// <param name="libraryFileMap"></param>
		/// <param name="contentDirectory"></param>
		public SDL2Renderer(Vector2i windowSize, Dictionary<string, string> libraryFileMap = null, string contentDirectory = null)
		{
			SpriteCache.Initialize(libraryFileMap, contentDirectory);

			if (SDL2.SDL.SDL_Init(SDL2.SDL.SDL_INIT_VIDEO) < 0)
				throw new Exception($"Failed to initialize SDL2 library. SDL2Error({ SDL2.SDL.SDL_GetError() })");

			WindowPtr = SDL2.SDL.SDL_CreateWindow("sdl .net core test",
				SDL2.SDL.SDL_WINDOWPOS_CENTERED, SDL2.SDL.SDL_WINDOWPOS_CENTERED,
				windowSize.X, windowSize.Y, SDL2.SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL);
			if (WindowPtr == IntPtr.Zero)
				throw new Exception($"Failed to create window. SDL2Error({ SDL2.SDL.SDL_GetError() })");

			RendererPtr = SDL2.SDL.SDL_CreateRenderer(WindowPtr, -1, 
				/*SDL2.SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC |*/ SDL2.SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
			if (RendererPtr == IntPtr.Zero)
				throw new Exception($"Failed to create renderer. SDL2Error({ SDL2.SDL.SDL_GetError() })");

			SDL2.SDL.SDL_SetRenderDrawBlendMode(RendererPtr, SDL2.SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
		}

		public override void Initialize()
		{
			CurrentFont = SpriteCache.LoadFont("font-5x7");
			SpriteCache.CreateTexture(CurrentFont.SpriteId, out _);
		}

		public override void Destroy()
		{
			foreach (var buffer in ScreenBuffers.Values)
			{
				if (buffer.Allocated())
				{
					SDL2.SDL.SDL_FreeSurface(buffer.SdlSurfacePtr);
					Marshal.FreeHGlobal(buffer.UnmanagedPtr);
				}
			}

			SpriteCache.Destroy();

			SDL2.SDL.SDL_DestroyRenderer(RendererPtr);
			SDL2.SDL.SDL_DestroyWindow(WindowPtr);

			Log.Add($"SDL2Renderer.Destroy()");
		}

		public override void Render(Engine.Core.Game simulation, double interpolation)
		{
			var start = Engine.Core.GameLoop.GetElapsed();

			// clear backbuffer
			SDL2.SDL.SDL_SetRenderDrawColor(RendererPtr, 0x0, 0x0, 0x0, 0xff);
			SDL2.SDL.SDL_RenderClear(RendererPtr);

			simulation.Render(interpolation);

			// display backbuffer
			SDL2.SDL.SDL_RenderPresent(RendererPtr);

			FramesRendered++;
			LastFrameDuration = Engine.Core.GameLoop.GetElapsed() - start;
		}

		public override void SetWindowTitle(string title)
		{
			SDL2.SDL.SDL_SetWindowTitle(WindowPtr, title);
		}

		public override void RenderRectangle(AABB view, AABB rectangle, Color4b borderColor, Color4b fillColor)
		{
			var scale = CurrentWindowSize / view.Size;
			var screenSpace = (rectangle.Min - view.Min) * scale;

			var rect = new SDL2.SDL.SDL_Rect
			{
				x = (int)screenSpace.X,
				y = (int)((view.Size.Y - rectangle.Size.Y) * scale.Y - screenSpace.Y),
				w = (int)(rectangle.Size.X * scale.X),
				h = (int)(rectangle.Size.Y * scale.Y),
			};

			if (fillColor.A > 0)
			{
				// fill rectangle
				SDL2.SDL.SDL_SetRenderDrawColor(RendererPtr, fillColor.R, fillColor.G, fillColor.B, fillColor.A);
				SDL2.SDL.SDL_RenderFillRect(RendererPtr, ref rect);
			}

			if (borderColor != fillColor)
			{
				if (borderColor.A > 0)
				{
					// border rectangle
					SDL2.SDL.SDL_SetRenderDrawColor(RendererPtr, borderColor.R, borderColor.G, borderColor.B, borderColor.A);
					SDL2.SDL.SDL_RenderDrawRect(RendererPtr, ref rect);
				}
			}
		}

		public override void RenderRectangleOverlay(AABB view, AABB rectangle, Color4b fillColor)
		{
			var scale = CurrentWindowSize / view.Size;
			var rect = new SDL2.SDL.SDL_Rect
			{
				x = (int)(rectangle.Left * scale.X),
				y = (int)(rectangle.Bottom * scale.Y),
				w = (int)(rectangle.Size.X * scale.X),
				h = (int)(rectangle.Size.Y * scale.Y),
			};

			SDL2.SDL.SDL_SetRenderDrawColor(RendererPtr, fillColor.R, fillColor.G, fillColor.B, fillColor.A);
			SDL2.SDL.SDL_RenderFillRect(RendererPtr, ref rect);
		}

		/// <summary>
		/// render colored line between point a and b
		/// </summary>
		/// <param name="view"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="color"></param>
		public override void RenderLine(AABB view, Vector2f a, Vector2f b, Color4b color)
		{
			var scale = CurrentWindowSize / view.Size;

			// line points in screen space
			a = (a - view.Min) * scale;
			b = (b - view.Min) * scale;

			SDL2.SDL.SDL_SetRenderDrawColor(RendererPtr, color.R, color.G, color.B, color.A);
			SDL2.SDL.SDL_RenderDrawLine(RendererPtr,
				(int)a.X, (int)(view.Size.Y * scale.Y - a.Y),
				(int)b.X, (int)(view.Size.Y * scale.Y - b.Y));
		}

		/// <summary>
		/// render colored line between point a and b (screenspace)
		/// both points have origin in topleft corner of screen
		/// </summary>
		/// <param name="view"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="color"></param>
		public override void RenderLineOverlay(AABB view, Vector2i a, Vector2i b, Color4b color)
		{
			var scale = CurrentWindowSize / view.Size;

			var af = a * scale;
			var bf = b * scale;

			SDL2.SDL.SDL_SetRenderDrawColor(RendererPtr, color.R, color.G, color.B, color.A);
			SDL2.SDL.SDL_RenderDrawLine(RendererPtr,
				(int)af.X, (int)af.Y,
				(int)bf.X, (int)bf.Y);
		}

		/// <summary>
		/// render sprite to screen
		/// </summary>
		/// <param name="view"></param>
		/// <param name="sprite">width == height, if not sprite is considered as animation strip</param>
		/// <param name="spritePosition">sprite position in world (bottomLeft)</param>
		public override void RenderSpriteFrame(AABB view, Sprite sprite, Vector2f spritePosition, int frame)
		{
			if (sprite == null)
				return;

			var scale = CurrentWindowSize / view.Size;
			var screenSpace = (spritePosition - view.Min) * scale;
			var spriteSize = sprite.Size.Y;

			var sourceRect = new SDL2.SDL.SDL_Rect
			{
				x = spriteSize * frame,
				y = 0,
				w = spriteSize,
				h = spriteSize
			};
			var destinationRect = new SDL2.SDL.SDL_Rect
			{
				x = (int)screenSpace.X,
				y = (int)((view.Size.Y - spriteSize) * scale.Y - screenSpace.Y),
				w = (int)(spriteSize * scale.X),
				h = (int)(spriteSize * scale.Y)
			};
			SDL2.SDL.SDL_RenderCopy(RendererPtr, sprite.TexturePtr, ref sourceRect, ref destinationRect);
		}

		public override void RenderSprite(AABB view, Sprite sprite, Vector2f spritePosition)
		{
			if (sprite == null)
				return;

			var scale = CurrentWindowSize / view.Size;
			var screenSpace = (spritePosition - view.Min) * scale;

			var sourceRect = new SDL2.SDL.SDL_Rect
			{
				x = 0,
				y = 0,
				w = sprite.Size.X,
				h = sprite.Size.Y
			};
			var destinationRect = new SDL2.SDL.SDL_Rect
			{
				x = (int)screenSpace.X,
				y = (int)((view.Size.Y - sprite.Size.Y) * scale.Y - screenSpace.Y),
				w = (int)(sprite.Size.X * scale.X),
				h = (int)(sprite.Size.Y * scale.Y)
			};
			SDL2.SDL.SDL_RenderCopy(RendererPtr, sprite.TexturePtr, ref sourceRect, ref destinationRect);
		}

		/// <summary>
		/// render text lines in screen space
		/// </summary>
		/// <param name="view"></param>
		/// <param name="position">origin in topleft</param>
		/// <param name="lines"></param>
		/// <param name="color"></param>
		/// <param name="fontSize"></param>
		public override void RenderTextLinesOverlay(AABB view, Vector2i position, TextLine[] lines, 
			double fontSize = 1)
		{
			var font = CurrentFont;
			if (font == null)
				return;

			var fontSprite = SpriteCache.Get(font.SpriteId);

			var scale = CurrentWindowSize / view.Size * fontSize;

			var sourceRect = new SDL2.SDL.SDL_Rect();
			var destinationRect = new SDL2.SDL.SDL_Rect();

			var textPosition = position;
			foreach (var line in lines)
			{
				if (line is Text text)
				{
					SDL2.SDL.SDL_SetTextureColorMod(fontSprite.TexturePtr, text.Color.R, text.Color.G, text.Color.B);

					for (var i = 0; i < text.Value.Length; i++)
					{
						var charIndex = (int)text.Value[i] - 32;
						var charAabb = font.CharBoxes[charIndex];

						sourceRect.x = font.CharSize.X * charIndex + (int)charAabb.Min.X;
						sourceRect.y = (int)charAabb.Min.Y;
						sourceRect.w = (int)charAabb.Size.X;
						sourceRect.h = (int)charAabb.Size.Y;

						destinationRect.x = (int)(textPosition.X * scale.X);
						destinationRect.y = (int)(textPosition.Y * scale.Y);
						destinationRect.w = (int)(charAabb.Size.X * scale.X);
						destinationRect.h = (int)(charAabb.Size.Y * scale.Y);

						SDL2.SDL.SDL_RenderCopy(RendererPtr, fontSprite.TexturePtr, ref sourceRect, ref destinationRect);
						textPosition.X += (int)charAabb.Size.X + font.Spacing.X;
					}

					textPosition.Y += font.CharSize.Y + font.Spacing.Y;
					textPosition.X = position.X;
				}
				else if (line is EmptyLine)
				{
					textPosition.Y += font.CharSize.Y + font.Spacing.Y;
				}
			}
		}

		public override Vector2i GetTextSize(TextLine[] lines, Font font = null)
		{
			var size = Vector2i.Zero;

			font ??= CurrentFont;
			if (font == null)
				return size;

			foreach (var line in lines)
			{
				if (line is Text text)
				{
					var lineLength = 0;
					for (var i = 0; i < text.Value.Length; i++)
					{
						var charIndex = (int)text.Value[i] - 32;
						var charAabb = font.CharBoxes[charIndex];

						lineLength += (int)charAabb.Size.X + font.Spacing.X;
					}
					size.X = Math.Max(lineLength, size.X);
					size.Y += font.CharSize.Y + font.Spacing.Y;
				}
				else if (line is EmptyLine)
				{
					size.Y += font.CharSize.Y + font.Spacing.Y;
				}
			}

			return size;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="layer"></param>
		/// <param name="view"></param>
		public override void RenderLayer(AABB view, Layer layer)
		{
			var colorSprite = SpriteCache.Get(layer.ColorMapId);

			var sourceRect = new SDL2.SDL.SDL_Rect
			{
				x = (int)view.Left,
				y = (int)(colorSprite.Size.Y - view.Top),
				w = (int)view.Size.X,
				h = (int)view.Size.Y
			};

			SDL2.SDL.SDL_RenderCopy(RendererPtr, colorSprite.TexturePtr, ref sourceRect, IntPtr.Zero);
		}
	}
}
