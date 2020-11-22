using SDL2;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Entities;
using Ujeby.Plosinofka.Engine.Graphics;

namespace Ujeby.Plosinofka.Game.Graphics
{
	internal enum BufferEnum : int
	{
		PerPixelShading = 0,
		AmbientOcclusion1,
		AmbientOcclusion2,
		DirectIllumination,
	}

	internal struct ScreenBuffer
	{
		public byte[] Data;
		public IntPtr SdlSurfacePtr;
		public IntPtr UnmanagedPtr;

		public bool Allocated() => Data != null && UnmanagedPtr != IntPtr.Zero && SdlSurfacePtr != IntPtr.Zero;
	}

	internal class SDL2Renderer : Renderer
	{
		internal IntPtr WindowPtr { get; private set; }
		internal IntPtr RendererPtr { get; private set; }

		public override Vector2i CurrentWindowSize
		{
			get
			{
				SDL.SDL_GetWindowSize(WindowPtr, out int width, out int height);
				return new Vector2i(width, height);
			}
		}

		/// <summary>max number of skippable frames to render</summary>
		public override int MaxFrameSkip { get; protected set; } = 5;
		public override double LastFrameDuration { get; protected set; }
		public override double LastShadingDuration { get; protected set; }
		public override long FramesRendered { get; protected set; } = 0;

		private Font CurrentFont;

		private readonly Dictionary<BufferEnum, ScreenBuffer> ScreenBuffers = new Dictionary<BufferEnum, ScreenBuffer>();

		public override Font GetCurrentFont() => CurrentFont;

		private ScreenBuffer CreateScreenBuffer(BufferEnum id, Vector2i size)
		{
			var bufferFound = ScreenBuffers.TryGetValue(id, out ScreenBuffer buffer);

			if (!bufferFound || size.Area() * 4 != buffer.Data?.Length)
			{
				// different size
				if (bufferFound && buffer.Allocated())
				{
					// free old buffer
					SDL.SDL_FreeSurface(buffer.SdlSurfacePtr);
					Marshal.FreeHGlobal(buffer.UnmanagedPtr);
				}

				// create new buffer
				buffer = new ScreenBuffer
				{
					Data = new byte[size.Area() * 4]
				};
				buffer.UnmanagedPtr = Marshal.AllocHGlobal(buffer.Data.Length);
				buffer.SdlSurfacePtr = SDL.SDL_CreateRGBSurfaceFrom(buffer.UnmanagedPtr,
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

		public SDL2Renderer(Vector2i windowSize)
		{
			if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
				throw new Exception($"Failed to initialize SDL2 library. SDL2Error({ SDL.SDL_GetError() })");

			WindowPtr = SDL.SDL_CreateWindow("sdl .net core test",
				SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED,
				windowSize.X, windowSize.Y, SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL);
			if (WindowPtr == null)
				throw new Exception($"Failed to create window. SDL2Error({ SDL.SDL_GetError() })");

			RendererPtr = SDL.SDL_CreateRenderer(WindowPtr, -1, 
				/*SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC |*/ SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
			if (RendererPtr == null)
				throw new Exception($"Failed to create renderer. SDL2Error({ SDL.SDL_GetError() })");

			SDL.SDL_SetRenderDrawBlendMode(RendererPtr, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
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
					SDL.SDL_FreeSurface(buffer.SdlSurfacePtr);
					Marshal.FreeHGlobal(buffer.UnmanagedPtr);
				}
			}

			SpriteCache.Destroy();

			SDL.SDL_DestroyRenderer(RendererPtr);
			SDL.SDL_DestroyWindow(WindowPtr);

			Log.Add($"SDL2Renderer.Destroy()");
		}

		public override void Render(Engine.Core.Game simulation, double interpolation)
		{
			var start = Engine.Core.GameLoop.GetElapsed();

			// clear backbuffer
			SDL.SDL_SetRenderDrawColor(RendererPtr, 0x0, 0x0, 0x0, 0xff);
			SDL.SDL_RenderClear(RendererPtr);

			simulation.Render(interpolation);

			// display backbuffer
			SDL.SDL_RenderPresent(RendererPtr);

			FramesRendered++;
			LastFrameDuration = Engine.Core.GameLoop.GetElapsed() - start;
		}

		public override void SetWindowTitle(string title)
		{
			SDL.SDL_SetWindowTitle(WindowPtr, title);
		}

		public override void RenderRectangle(AABB view, AABB rectangle, Color4b borderColor, Color4b fillColor)
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

			if (fillColor.A > 0)
			{
				// fill rectangle
				SDL.SDL_SetRenderDrawColor(RendererPtr, fillColor.R, fillColor.G, fillColor.B, fillColor.A);
				SDL.SDL_RenderFillRect(RendererPtr, ref rect);
			}

			if (borderColor != fillColor)
			{
				if (borderColor.A > 0)
				{
					// border rectangle
					SDL.SDL_SetRenderDrawColor(RendererPtr, borderColor.R, borderColor.G, borderColor.B, borderColor.A);
					SDL.SDL_RenderDrawRect(RendererPtr, ref rect);
				}
			}
		}

		public override void RenderRectangleOverlay(AABB view, AABB rectangle, Color4b fillColor)
		{
			var scale = CurrentWindowSize / view.Size;
			var rect = new SDL.SDL_Rect
			{
				x = (int)(rectangle.Left * scale.X),
				y = (int)(rectangle.Bottom * scale.Y),
				w = (int)(rectangle.Size.X * scale.X),
				h = (int)(rectangle.Size.Y * scale.Y),
			};

			SDL.SDL_SetRenderDrawColor(RendererPtr, fillColor.R, fillColor.G, fillColor.B, fillColor.A);
			SDL.SDL_RenderFillRect(RendererPtr, ref rect);
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

			SDL.SDL_SetRenderDrawColor(RendererPtr, color.R, color.G, color.B, color.A);
			SDL.SDL_RenderDrawLine(RendererPtr,
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

			SDL.SDL_SetRenderDrawColor(RendererPtr, color.R, color.G, color.B, color.A);
			SDL.SDL_RenderDrawLine(RendererPtr,
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

		public override void RenderSprite(AABB view, Sprite sprite, Vector2f spritePosition)
		{
			if (sprite == null)
				return;

			var scale = CurrentWindowSize / view.Size;
			var screenSpace = (spritePosition - view.Min) * scale;

			var sourceRect = new SDL.SDL_Rect
			{
				x = 0,
				y = 0,
				w = sprite.Size.X,
				h = sprite.Size.Y
			};
			var destinationRect = new SDL.SDL_Rect
			{
				x = (int)screenSpace.X,
				y = (int)((view.Size.Y - sprite.Size.Y) * scale.Y - screenSpace.Y),
				w = (int)(sprite.Size.X * scale.X),
				h = (int)(sprite.Size.Y * scale.Y)
			};
			SDL.SDL_RenderCopy(RendererPtr, sprite.TexturePtr, ref sourceRect, ref destinationRect);
		}

		/// <summary>
		/// render text lines in screen space
		/// </summary>
		/// <param name="view"></param>
		/// <param name="position">origin in topleft</param>
		/// <param name="lines"></param>
		/// <param name="color"></param>
		/// <param name="fontSize"></param>
		public override void RenderTextLinesOverlay(AABB view, Vector2i position, TextLine[] lines, Color4b color, 
			double fontSize = 1)
		{
			var font = CurrentFont;
			if (font == null)
				return;

			var fontSprite = SpriteCache.Get(font.SpriteId);

			var scale = CurrentWindowSize / view.Size * fontSize;

			var sourceRect = new SDL.SDL_Rect();
			var destinationRect = new SDL.SDL_Rect();

			var textPosition = position;
			foreach (var line in lines)
			{
				if (line is Text text)
				{
					// TODO RenderText color
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

						SDL.SDL_RenderCopy(RendererPtr, fontSprite.TexturePtr, ref sourceRect, ref destinationRect);
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

			var sourceRect = new SDL.SDL_Rect
			{
				x = (int)view.Left,
				y = (int)(colorSprite.Size.Y - view.Top),
				w = (int)view.Size.X,
				h = (int)view.Size.Y
			};

			SDL.SDL_RenderCopy(RendererPtr, colorSprite.TexturePtr, ref sourceRect, IntPtr.Zero);
		}

		public override void RenderLayer(AABB view, Layer layer, Light[] lights, AABB[] obstacles)
		{
			var shadingStart = Engine.Core.GameLoop.GetElapsed();

			if (Settings.Instance.GetVisual(VisualSetting.PerPixelShading) && layer.DataMapId != null)
			{
				var buffer = DirectLighting(view, layer, lights, obstacles);
				//var buffer = AmbientOcclusion(view, layer, lights, obstacles, 2);
				//var buffer = PerPixelShading(view, layer, lights, obstacles);

				// copy to data to unmanaged array
				Marshal.Copy(buffer.Data, 0, buffer.UnmanagedPtr, buffer.Data.Length);

				// create texture
				var texture = SDL.SDL_CreateTextureFromSurface(RendererPtr, buffer.SdlSurfacePtr);

				// draw layer
				SDL.SDL_RenderCopy(RendererPtr, texture, IntPtr.Zero, IntPtr.Zero);
				SDL.SDL_DestroyTexture(texture);
			}
			else
				RenderLayer(view, layer);

			LastShadingDuration = Engine.Core.GameLoop.GetElapsed() - shadingStart;
		}

		private ScreenBuffer PerPixelShading(AABB view, Layer layer, Light[] lights, AABB[] obstacles)
		{
			var colorLayer = SpriteCache.Get(layer.ColorMapId);

			var viewMin = view.Min.Round();
			var viewSize = view.Size.Round();

			var buffer = CreateScreenBuffer(BufferEnum.PerPixelShading, viewSize);
			var bufferLength = buffer.Data.Length / 4;

			//for (var i = 0; i < bufferLength; i++)
			Parallel.For(0, bufferLength, (i, loopState) =>
			{
				var bufferPixel = new Vector2i(i % viewSize.X, i / viewSize.X);
				var layerIndex = (viewMin.Y + bufferPixel.Y) * colorLayer.Size.X + viewMin.X + bufferPixel.X;
				var bufferIndex = ((viewSize.Y - bufferPixel.Y - 1) * viewSize.X + bufferPixel.X) * 4;

				var color = new Color4f(colorLayer.Data[layerIndex]);
				if (color.A > 0)
				{
					// TODO per pixel shading
					color.A *= 0.5;

					var finalColor = new Color4b(color);
					buffer.Data[bufferIndex + 0] = finalColor.A;
					buffer.Data[bufferIndex + 1] = finalColor.B;
					buffer.Data[bufferIndex + 2] = finalColor.G;
					buffer.Data[bufferIndex + 3] = finalColor.R;
				}
				else
					// just alpha channel
					buffer.Data[bufferIndex] = 0;
			}
			);

			return buffer;
		}

		private ScreenBuffer DirectLighting(AABB view, Layer layer, Light[] lights, AABB[] obstacles)
		{
			var data = SpriteCache.Get(layer.DataMapId);
			var color = SpriteCache.Get(layer.ColorMapId);

			var viewMin = view.Min.Round();
			var viewSize = view.Size.Round();

			var result = CreateScreenBuffer(BufferEnum.DirectIllumination, viewSize);

			//for (var i = 0; i < result.Data.Length / 4; i++)
			Parallel.For(0, result.Data.Length / 4, (i, loopState) =>
			{
				var screen = new Vector2i(i % viewSize.X, i / viewSize.X);
				var wIndex = (viewMin.Y + screen.Y) * data.Size.X + viewMin.X + screen.X;
				var sIndex = ((viewSize.Y - screen.Y - 1) * viewSize.X + screen.X) * 4;

				if ((data.Data[wIndex] & Level.ShadeMask) == Level.ShadeMask)
				{
					var tmpColor = new Color4f(color.Data[wIndex]);
					if (tmpColor.A > 0)
						tmpColor += RayTracing.DirectLight(screen + viewMin, lights, obstacles) * 0.5;

					var finalColor = new Color4b(tmpColor);
					result.Data[sIndex + 0] = finalColor.A;
					result.Data[sIndex + 1] = finalColor.B;
					result.Data[sIndex + 2] = finalColor.G;
					result.Data[sIndex + 3] = finalColor.R;
				}
				else
					// just alpha channel
					result.Data[sIndex] = 0;
			}
			);

			return result;
		}

		private ScreenBuffer AmbientOcclusion(AABB view, Layer layer, Light[] lights, AABB[] obstacles, int pixelSize)
		{
			var data = SpriteCache.Get(layer.DataMapId);
			var color = SpriteCache.Get(layer.ColorMapId);

			var viewMin = view.Min.Round();
			var viewSize = view.Size.Round();

			// ambient occlusion
			var aoSize = viewSize / pixelSize;
			var aoBuffer = CreateScreenBuffer(BufferEnum.AmbientOcclusion1, aoSize);
			var aoBufferLength = aoBuffer.Data.Length / 4;
			Parallel.For(0, aoBufferLength, (i, loopState) =>
			{
				var aoPixel = new Vector2i(i % aoSize.X, i / aoSize.X);
				var aoIndex = ((aoSize.Y - aoPixel.Y - 1) * aoSize.X + aoPixel.X) * 4;
				var screenPixel = aoPixel * pixelSize + new Vector2i(pixelSize / 2);

				var pixelColor = RayTracing.AmbientOcclusion(viewMin + screenPixel, obstacles,
					probeDistance: 128, rayCount: 4);

				var finalColor = new Color4b(pixelColor);
				aoBuffer.Data[aoIndex + 0] = finalColor.A;
				aoBuffer.Data[aoIndex + 1] = finalColor.B;
				aoBuffer.Data[aoIndex + 2] = finalColor.G;
				aoBuffer.Data[aoIndex + 3] = finalColor.R;
			});

			// ambient occlusion & color
			var viewBuffer = CreateScreenBuffer(BufferEnum.AmbientOcclusion1, viewSize);
			var viewBufferLength = viewBuffer.Data.Length / 4;
			Parallel.For(0, viewBufferLength, (i, loopState) =>
			{
				var viewPixel = new Vector2i(i % viewSize.X, i / viewSize.X);
				var viewIndex = ((viewSize.Y - viewPixel.Y - 1) * viewSize.X + viewPixel.X) * 4;

				var colorIndex = (viewMin.Y + viewPixel.Y) * data.Size.X + viewMin.X + viewPixel.X;
				if (color.Data[colorIndex + 0] > 0)
				{
					var aoPixel = viewPixel / pixelSize;
					var aoIndex = ((aoSize.Y - aoPixel.Y - 1) * aoSize.X + aoPixel.X) * 4;

					var pixelColor = new Color4f(color.Data[colorIndex]);
					var aoColor = new Color4f(new Color4b(
						aoBuffer.Data[aoIndex + 3],
						aoBuffer.Data[aoIndex + 2],
						aoBuffer.Data[aoIndex + 1],
						aoBuffer.Data[aoIndex + 0]).AsUint);

					var finalColor = new Color4b(pixelColor * aoColor);
					viewBuffer.Data[viewIndex + 0] = finalColor.A;
					viewBuffer.Data[viewIndex + 1] = finalColor.B;
					viewBuffer.Data[viewIndex + 2] = finalColor.G;
					viewBuffer.Data[viewIndex + 3] = finalColor.R;
				}
				else
					// just alpha channel
					viewBuffer.Data[viewIndex] = 0;
			});

			return viewBuffer;
		}
	}
}
