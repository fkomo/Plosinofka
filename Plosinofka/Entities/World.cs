using SDL2;
using System;

namespace Ujeby.Plosinofka.Entities
{
	class Level
	{
		public Vector2i Size;
		public Guid CollisionTextureId;
		public Guid ColorTextureId;
	}

	class World
	{
		public Level CurrentLevel { get; private set; }

		public World()
		{
			var colorTexture = TextureCache.Load(@".\Content\Worlds\world1-color.png");
			var collisionTexture = TextureCache.Load(@".\Content\Worlds\world1-collision.png");

			CurrentLevel = new Level
			{
				Size = colorTexture.Size,
				CollisionTextureId = collisionTexture.Id,
				ColorTextureId = colorTexture.Id,
			};
		}

		public void Render(Camera camera, World beforeUpdate, double interpolation)
		{
			// SDL starts at top left
			// world starts at bottom left
			var colorTexture = TextureCache.Get(CurrentLevel.ColorTextureId);
			if (colorTexture == null)
				return;

			var cameraPosition = camera.GetPosition(interpolation);

			var source = new SDL.SDL_Rect
			{
				x = (int)(cameraPosition.X - (camera.Size.X / 2)),
				y = colorTexture.Size.Y - (int)(cameraPosition.Y + (camera.Size.Y / 2)),
				w = camera.Size.X,
				h = camera.Size.Y
			};

			SDL.SDL_RenderCopy(Renderer.Instance.RendererPtr, colorTexture.Ptr, ref source, IntPtr.Zero);
		}
	}
}
