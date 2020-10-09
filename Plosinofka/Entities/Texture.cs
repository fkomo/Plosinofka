using SDL2;
using System;
using System.Collections.Generic;

namespace Ujeby.Plosinofka.Entities
{
	class Texture
	{
		public Guid Id;
		public Vector2i Size;
		public IntPtr Ptr = IntPtr.Zero;
	}

	class TextureCache
	{
		private static Dictionary<Guid, Texture> Textures = new Dictionary<Guid, Texture>();

		public static Texture Load(string fileName)
		{
			var imagePtr = SDL_image.IMG_Load(fileName);
			var texturePtr = SDL.SDL_CreateTextureFromSurface(Renderer.Instance.RendererPtr, imagePtr);
			SDL.SDL_FreeSurface(imagePtr);

			var texture = new Texture
			{
				Id = Guid.NewGuid(),
				Ptr = texturePtr,
				Size = new Vector2i(1920, 1080) // TODO get size of loaded image
			};

			Textures.Add(texture.Id, texture);

			return texture;
		}

		public static void Destroy()
		{
			foreach (var texture in Textures)
				SDL.SDL_DestroyTexture(texture.Value.Ptr);

			Textures.Clear();
		}

		internal static Texture Get(Guid textureId)
		{
			if (!Textures.TryGetValue(textureId, out Texture texture))
				Log.Add($"WARNING | Texture({ textureId }) not found");

			return texture;
		}
	}
}
