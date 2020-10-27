﻿using SDL2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;

namespace Ujeby.Plosinofka.Graphics
{
	public class Resource
	{
		/// <summary>
		/// internal resource id
		/// </summary>
		public Guid Id;
	}

	public class Sprite : Resource
	{
		/// <summary></summary>
		public string Filename;

		/// <summary>width x height</summary>
		public Vector2i Size;
		
		/// <summary>
		/// pixel format: 0xARGB
		/// topLeft -> bottomRight
		/// </summary>
		public uint[] Data;

		/// <summary>sdl texture pointer</summary>
		public IntPtr TexturePtr = IntPtr.Zero;
	}

	/// <summary>
	/// horizontal collection of animation frames in single sprite
	/// </summary>
	public class AnimationSprite : Sprite
	{
		/// <summary>frame count</summary>
		public int Frames => Size.X / FrameSize.X;

		/// <summary></summary>
		public Vector2i FrameSize;
	}

	class ResourceCache
	{
		private static Dictionary<Guid, Resource> Resources = new Dictionary<Guid, Resource>();

		public static Sprite LoadSprite(string filename, bool copyPixelData = false)
		{
			var start = Game.GetElapsed();

			var sprite = CreateSprite(filename, copyPixelData);
			Resources.Add(sprite.Id, sprite);

			Log.Add($"LoadSprite('{ filename }', data:{ copyPixelData }): { (int)(Game.GetElapsed() - start) }ms");
			return sprite;
		}

		public static Sprite LoadAnimationSprite(string filename, bool copyPixelData = false)
		{
			var start = Game.GetElapsed();

			var sprite = CreateSprite(filename, copyPixelData);
			if (sprite == null)
				return null;

			var animationSprite = new AnimationSprite
			{
				Id = sprite.Id,
				TexturePtr = sprite.TexturePtr,
				Size = sprite.Size,
				Filename = sprite.Filename,
				FrameSize = new	Vector2i(sprite.Size.Y, sprite.Size.Y)
			};

			Resources.Add(animationSprite.Id, animationSprite);

			Log.Add($"LoadAnimationSprite('{ filename }', data:{ copyPixelData }): { animationSprite.Frames } frames; { (int)(Game.GetElapsed() - start) }ms");
			return animationSprite;
		}

		private static Sprite CreateSprite(string filename, bool copyPixelData)
		{
			if (!File.Exists(filename))
			{
				Log.Add($"CreateSprite('{ filename }'): file not found!");
				return null;
			}

			var imagePtr = SDL_image.IMG_Load(filename);
			var surface = Marshal.PtrToStructure<SDL.SDL_Surface>(imagePtr);
			var texturePtr = SDL.SDL_CreateTextureFromSurface(Renderer.Instance.RendererPtr, imagePtr);

			var sprite = new Sprite
			{
				Id = Guid.NewGuid(),
				TexturePtr = texturePtr,
				Size = new Vector2i(surface.w, surface.h),
				Filename = filename,
			};

			if (copyPixelData)
			{
				//var bitmap = new Bitmap(fileName);
				//var data = bitmap.LockBits(
				//	new Rectangle(Point.Empty, bitmap.Size), 
				//	System.Drawing.Imaging.ImageLockMode.ReadWrite,
				//	bitmap.PixelFormat);

				//sprite.Data = new byte[data.Height * data.Stride];
				//Marshal.Copy(data.Scan0, sprite.Data, 0, sprite.Data.Length);

				var tmpData = new byte[surface.w * surface.h * 4];
				Marshal.Copy(surface.pixels, tmpData, 0, tmpData.Length);

				var i2 = 0;
				sprite.Data = new uint[surface.w * surface.h];
				for (var y = surface.h - 1; y >= 0; y--)
					for (var x = 0; x < surface.w; x++, i2++)
					{
						var i = y * surface.w + x;
						sprite.Data[i2] =
							((uint)tmpData[i * 4 + 0]) +
							((uint)tmpData[i * 4 + 1] << 8) +
							((uint)tmpData[i * 4 + 2] << 16) +
							((uint)tmpData[i * 4 + 3] << 24);
					}
			}

			SDL.SDL_FreeSurface(imagePtr);

			return sprite;
		}


		public static void Destroy()
		{
			// free sdl textures
			foreach (Sprite texture in Resources.Values.Where(r => (r as Sprite)?.TexturePtr != IntPtr.Zero))
				SDL.SDL_DestroyTexture(texture.TexturePtr);

			Resources.Clear();
		}

		internal static T Get<T>(Guid resourceId) where T : Resource
		{
			if (resourceId == Guid.Empty)
				return default;

			if (!Resources.TryGetValue(resourceId, out Resource resource))
				Log.Add($"WARNING | Resource({ resourceId }) not found");

			return (T)resource;
		}
	}
}
