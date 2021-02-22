using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Entities;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Engine.SDL;

namespace Ujeby.Plosinofka.Game.Graphics
{
	internal class Rendering
	{
		internal static void Render(AABB view, Layer layer, Light[] lights, AABB[] obstacles)
		{
			var sdl2Renderer = Renderer.Instance as SDL2Renderer;

			if (Settings.Instance.GetVisual(VisualSetting.PerPixelShading) && layer.DataMapId != null)
			{
				var buffer = DirectLighting(view, layer, lights, obstacles);
				//var buffer = AmbientOcclusion(view, layer, lights, obstacles, 2);
				//var buffer = PerPixelShading(view, layer, lights, obstacles);

				// copy to data to unmanaged array
				Marshal.Copy(buffer.Data, 0, buffer.UnmanagedPtr, buffer.Data.Length);

				// create texture
				var texture = SDL2.SDL.SDL_CreateTextureFromSurface(sdl2Renderer.RendererPtr, buffer.SdlSurfacePtr);

				// draw layer
				SDL2.SDL.SDL_RenderCopy(sdl2Renderer.RendererPtr, texture, IntPtr.Zero, IntPtr.Zero);
				SDL2.SDL.SDL_DestroyTexture(texture);
			}
			else
				Renderer.Instance.RenderLayer(view, layer);
		}

		private static ScreenBuffer PerPixelShading(AABB view, Layer layer, Light[] lights, AABB[] obstacles)
		{
			var colorLayer = SpriteCache.Get(layer.ColorMapId);

			var viewMin = view.Min.Round();
			var viewSize = view.Size.Round();

			var sdl2Renderer = Renderer.Instance as SDL2Renderer;
			var buffer = sdl2Renderer.CreateScreenBuffer(BufferEnum.PerPixelShading, viewSize);
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

		private static ScreenBuffer DirectLighting(AABB view, Layer layer, Light[] lights, AABB[] obstacles)
		{
			var data = SpriteCache.Get(layer.DataMapId);
			var color = SpriteCache.Get(layer.ColorMapId);

			var viewMin = view.Min.Round();
			var viewSize = view.Size.Round();

			var sdl2Renderer = Renderer.Instance as SDL2Renderer;
			var result = sdl2Renderer.CreateScreenBuffer(BufferEnum.DirectIllumination, viewSize);

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

		private static ScreenBuffer AmbientOcclusion(AABB view, Layer layer, Light[] lights, AABB[] obstacles, int pixelSize)
		{
			var data = SpriteCache.Get(layer.DataMapId);
			var color = SpriteCache.Get(layer.ColorMapId);

			var viewMin = view.Min.Round();
			var viewSize = view.Size.Round();

			var sdl2Renderer = Renderer.Instance as SDL2Renderer;

			// ambient occlusion
			var aoSize = viewSize / pixelSize;
			var aoBuffer = sdl2Renderer.CreateScreenBuffer(BufferEnum.AmbientOcclusion1, aoSize);
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
			var viewBuffer = sdl2Renderer.CreateScreenBuffer(BufferEnum.AmbientOcclusion1, viewSize);
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
