using SDL2;
using System;
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka.Entities
{
	class Level
	{
		public Vector2i Size;
		public Guid CollisionResourceId;
		public Guid BackgroundTextureId;
	}

	class World
	{
		public Level CurrentLevel { get; private set; }

		public World()
		{
			var colorTexture = ResourceCache.LoadSprite(@".\Content\Worlds\world1-color.png");
			CurrentLevel = new Level
			{
				Size = colorTexture.Size,
				BackgroundTextureId = colorTexture.Id,
			};

			//var test = ResourceCache.LoadSprite(@".\Content\test.png", true);

			var collisionResource = ResourceCache.LoadSprite(@".\Content\Worlds\world1-collision.png", true);
			CurrentLevel.CollisionResourceId = collisionResource.Id;
		}

		public void Update()
		{

		}

		public void RenderBackground(Camera camera, double interpolation)
		{
			var texture = ResourceCache.Get<Sprite>(CurrentLevel.BackgroundTextureId);
			if (texture == null)
				return;

			var cameraPosition = camera.GetPosition(interpolation);

			var source = new SDL.SDL_Rect
			{
				x = (int)cameraPosition.X,
				y = texture.Size.Y - (int)cameraPosition.Y, // because sdl surface starts at topleft
				w = camera.View.X,
				h = camera.View.Y
			};
			SDL.SDL_RenderCopy(Renderer.Instance.RendererPtr, texture.TexturePtr, ref source, IntPtr.Zero);
		}

		public void RenderForeground(Camera camera, double interpolation)
		{
			// TODO render world foreground
		}
	}
}
