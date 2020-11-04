using SDL2;
using System;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Game.Graphics;

namespace Ujeby.Plosinofka.Game
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				var renderer = new SDL2Renderer(Vector2i.FullHD);
				var simulation = new Simulation0();
				var input = new SDL2Input();

				new Engine.Core.Game("Plosinofka", renderer, simulation, input).Run();

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
