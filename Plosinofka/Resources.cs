﻿using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka.Entities
{
	class Resource
	{
		/// <summary>
		/// internal resource id
		/// </summary>
		public Guid Id;
	}

	class Sprite : Resource
	{
		/// <summary>
		/// width x height
		/// </summary>
		public Vector2i Size;
		
		/// <summary>
		/// pixel format: RGBA
		/// topLeft -> bottomRight
		/// </summary>
		public byte[] Data;

		/// <summary>
		/// sdl texture pointer
		/// </summary>
		public IntPtr TexturePtr = IntPtr.Zero;
	}

	class ResourceCache
	{
		private static Dictionary<Guid, Resource> Resources = new Dictionary<Guid, Resource>();

		public static Sprite LoadSprite(string fileName, bool copyPixelData = false)
		{
			var imagePtr = SDL_image.IMG_Load(fileName);
			var surface = Marshal.PtrToStructure<SDL.SDL_Surface>(imagePtr);
			var texturePtr = SDL.SDL_CreateTextureFromSurface(Renderer.Instance.RendererPtr, imagePtr);

			var sprite = new Sprite
			{
				Id = Guid.NewGuid(),
				TexturePtr = texturePtr,
				Size = new Vector2i(surface.w, surface.h),
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

				sprite.Data = new byte[surface.w * surface.h * 4];
				Marshal.Copy(surface.pixels, sprite.Data, 0, sprite.Data.Length);

			}

			SDL.SDL_FreeSurface(imagePtr);

			Resources.Add(sprite.Id, sprite);
			return sprite;
		}

		public static void Destroy()
		{
			// free sdl textures
			foreach (Sprite texture in Resources.Values.Where(r => (r as Sprite)?.TexturePtr != IntPtr.Zero))
				SDL.SDL_DestroyTexture(texture.TexturePtr);

			// TODO free other resources

			Resources.Clear();
		}

		internal static T Get<T>(Guid resourceId) where T : Resource
		{
			if (!Resources.TryGetValue(resourceId, out Resource resource))
				Log.Add($"WARNING | Resource({ resourceId }) not found");

			return (T)resource;
		}
	}
}
