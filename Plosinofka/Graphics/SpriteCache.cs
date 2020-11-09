using SDL2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game.Entities;
using Ujeby.Plosinofka.Game.PlayerStates;

namespace Ujeby.Plosinofka.Game.Graphics
{
	public class SpriteCache
	{
		/// <summary>
		/// spriteId vs image path
		/// </summary>
		private static readonly Dictionary<string, string> LibraryFileMap = new Dictionary<string, string>()
		{
			{ PlayerDecals.DustParticlesLeft.ToString() , Program.ContentDirectory + "Effects\\DustParticlesLeft.png" },
			{ PlayerDecals.DustParticlesRight.ToString() , Program.ContentDirectory + "Effects\\DustParticlesRight.png" },

			{ PlayerAnimations.Idle.ToString() , Program.ContentDirectory + "Player\\player-idle.png" },
			{ PlayerAnimations.WalkingLeft.ToString() , Program.ContentDirectory + "Player\\player-walkingLeft.png" },
			{ PlayerAnimations.WalkingRight.ToString() , Program.ContentDirectory + "Player\\player-walkingRight.png" },
		};

		/// <summary>
		/// spriteId vs Sprite
		/// </summary>
		private static readonly Dictionary<string, Sprite> Library = new Dictionary<string, Sprite>();

		public static Sprite LoadSprite(string filename, string id = null)
		{
			var start = Engine.Core.Game.GetElapsed();

			var sprite = new Sprite
			{
				Filename = filename,
				Id = id ?? Guid.NewGuid().ToString("N"),
			};
			if (!LoadImage(filename, out sprite.TexturePtr, out sprite.Size, out sprite.Data))
				return null;

			Library.Add(sprite.Id, sprite);

			Log.Add($"LoadSprite('{ filename }'): { sprite.Id }; { (int)(Engine.Core.Game.GetElapsed() - start) }ms");
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
			texturePtr = SDL.SDL_CreateTextureFromSurface((Renderer.Instance as SDL2Renderer).RendererPtr, imagePtr);

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

			Log.Add($"SpriteCache.Destroy()");
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

		public static Font LoadFont(string name)
		{
			var file = Program.ContentDirectory + $"{ name }.png";
			if (!File.Exists(file))
			{
				Log.Add($"LoadFont('{ file }'): file not found!");
				return null;
			}

			var fileInfo = new FileInfo(file);
			var sizeString = fileInfo.Name
				.Replace(fileInfo.Extension, string.Empty)
				.Split("-").Last()
				.Split("x");

			var font = new Font
			{
				SpriteId = LoadSprite(file)?.Id,
				CharSize = new Vector2i(Convert.ToInt32(sizeString[0]), Convert.ToInt32(sizeString[1])),
				Spacing = new Vector2i(1, 1),
			};

			// create aabb's for each character
			var dataFile = Program.ContentDirectory + $"{ name }-data.png";
			if (File.Exists(dataFile))
			{
				var dataSprite = LoadSprite(dataFile);
				font.DataSpriteId = dataSprite.Id;

				font.CharBoxes = new AABB[dataSprite.Size.X / font.CharSize.X];
				for (var ci = 0; ci < dataSprite.Size.X; ci += font.CharSize.X)
				{
					var min = new Vector2i(font.CharSize.X, font.CharSize.Y);
					var max = new Vector2i(0, 0);

					for (var y = 0; y < font.CharSize.Y; y++)
					{
						for (var x = 0; x < font.CharSize.X; x++)
						{
							var index = y * dataSprite.Size.X + x + ci;
							if (dataSprite.Data[index] != 0)
							{
								min.X = Math.Min(min.X, x);
								min.Y = Math.Min(min.Y, y);
								max.X = Math.Max(max.X, x + 1);
								max.Y = Math.Max(max.Y, y + 1);
							}
						}
					}
					
					font.CharBoxes[ci / font.CharSize.X] = new AABB(min, max);
				}
			}

			return font;
		}
	}
}
