using SDL2;
using System;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Game.Graphics;

namespace Ujeby.Plosinofka.Game
{
	class Program
	{
		internal static string ContentDirectory = ".\\Content\\";

		static void Main()
		{
			try
			{
				var renderer = new SDL2Renderer(Vector2i.FullHD);
				var simulation = new Game0();
				var input = new SDL2Input();

				// game loop
				new Engine.Core.GameLoop("Plosinofka", renderer, simulation, input).Run();

				simulation.Destroy();
				renderer.Destroy();
				
				SDL.SDL_Quit();
			}
			catch (Exception ex)
			{
				Log.Add(ex.ToString());
			}
		}
	}
}
