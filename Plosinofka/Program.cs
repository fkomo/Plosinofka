using SDL2;
using System;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Network;
using Ujeby.Plosinofka.Engine.Network.Messages;
using Ujeby.Plosinofka.Engine.SDL;

namespace Ujeby.Plosinofka.Game
{
	class Program
	{
		internal static string ContentDirectory = ".\\Content\\";

		static void Main()
		{
			Game0 game = null;
			SDL2Input input = null;
			SDL2Renderer renderer = null;
			try
			{
				// renderer needs to be created first
				renderer = new SDL2Renderer(Vector2i.FullHD, Graphics.Sprites.LibraryFileMap, Program.ContentDirectory);

				game = new Game0();

				input = new SDL2Input() 
				{ 
					KeyboardButtonHandler = game.HandleKeyboardButton
				};

				// run game loop
				new GameLoop("Plosinofka", renderer, game, input)
					.Run();
			}
			catch (Exception ex)
			{
				Log.Add(ex.ToString());
			}
			finally
			{
				Client.Instance.Send(new LeaveGame { Login = game?.Player?.Name });

				try
				{
					game.Destroy();
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
}
