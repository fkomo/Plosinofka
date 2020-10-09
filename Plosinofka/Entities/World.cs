using SDL2;
using System;

namespace Ujeby.Plosinofka.Entities
{
	class World
	{
		public double Ground;

		public World(double ground)
		{
			Ground = ground;
		}

		public void Render(IntPtr renderer, World beforeUpdate, double interpolation)
		{
			SDL.SDL_SetRenderDrawColor(renderer, 128, 128, 128, 255);
			SDL.SDL_RenderDrawLine(renderer, 0, (int)Ground, 1920 / 2, (int)Ground);

			for (var i = 32; i < 1920/2; i += 32)
				SDL.SDL_RenderDrawLine(renderer, i, (int)Ground, i, 1080 / 2);
		}
	}
}
