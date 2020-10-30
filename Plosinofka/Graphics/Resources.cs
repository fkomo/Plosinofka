using SDL2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka.Graphics
{
	public class Sprite
	{
		/// <summary>internal sprite id</summary>
		public string Id;

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

	public class SpriteCache
	{
		private static readonly Dictionary<string, string> LibraryFileMap = new Dictionary<string, string>()
		{
			{ DecalsEnum.DustParticlesLeft.ToString() , ".\\Content\\Effects\\DustParticlesLeft.png" },
			{ DecalsEnum.DustParticlesRight.ToString() , ".\\Content\\Effects\\DustParticlesRight.png" },

			{ PlayerAnimations.WalkingLeft.ToString() , ".\\Content\\Player\\player-walkingLeft.png" },
			{ PlayerAnimations.WalkingRight.ToString() , ".\\Content\\Player\\player-walkingRight.png" },

		};

		private static Dictionary<string, Sprite> Library = new Dictionary<string, Sprite>();

		public static Sprite LoadSprite(string filename, string id = null)
		{
			var start = Game.GetElapsed();

			var sprite = new Sprite
			{
				Filename = filename,
				Id = id ?? Guid.NewGuid().ToString("N"),
			};
			if (!LoadImage(filename, out sprite.TexturePtr, out sprite.Size, out sprite.Data))
				return null;

			Library.Add(sprite.Id, sprite);

			Log.Add($"LoadSprite('{ filename }'): { id }; { (int)(Game.GetElapsed() - start) }ms");
			return sprite;
		}

		private static bool LoadImage(string filename, out IntPtr texturePtr, out Vector2i size, out uint[] data)
		{
			texturePtr = IntPtr.Zero;
			size = Vector2i.Zero;
			data = null;

			if (!File.Exists(filename))
			{
				Log.Add($"LoadImage('{ filename }'): file not found!");
				return false;
			}

			var imagePtr = SDL_image.IMG_Load(filename);
			var surface = Marshal.PtrToStructure<SDL.SDL_Surface>(imagePtr);
			texturePtr = SDL.SDL_CreateTextureFromSurface(Renderer.Instance.RendererPtr, imagePtr);

			size = new Vector2i(surface.w, surface.h);

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
			data = new uint[surface.w * surface.h];
			for (var y = surface.h - 1; y >= 0; y--)
				for (var x = 0; x < surface.w; x++, i2++)
				{
					var i = y * surface.w + x;
					data[i2] =
						((uint)tmpData[i * 4 + 0]) +
						((uint)tmpData[i * 4 + 1] << 8) +
						((uint)tmpData[i * 4 + 2] << 16) +
						((uint)tmpData[i * 4 + 3] << 24);
				}

			SDL.SDL_FreeSurface(imagePtr);

			return true;
		}

		public static void Destroy()
		{
			// free sdl textures
			foreach (var sprite in Library.Values.Where(s => s.TexturePtr != IntPtr.Zero))
				SDL.SDL_DestroyTexture(sprite.TexturePtr);

			Library.Clear();
		}

		internal static Sprite Get(string id)
		{
			if (string.IsNullOrEmpty(id))
				return null;

			if (!Library.TryGetValue(id, out Sprite sprite))
			{
				if (!LibraryFileMap.TryGetValue(id, out string filename))
					return null;
					
				return LoadSprite(filename, id);
			}

			return sprite;
		}
	}
}
