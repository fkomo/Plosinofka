using SDL2;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Core
{
	class Input
	{
		private static byte[] CurrentKeys = new byte[(int)SDL.SDL_Scancode.SDL_NUM_SCANCODES];
		private static byte[] PreviousKeys = new byte[(int)SDL.SDL_Scancode.SDL_NUM_SCANCODES];

		public static bool Handle(Simulation simulation)
		{
			while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
			{
				switch (e.type)
				{
					case SDL.SDL_EventType.SDL_QUIT:
						return false;
				};

				if (e.type == SDL.SDL_EventType.SDL_KEYUP || e.type == SDL.SDL_EventType.SDL_KEYDOWN)
				{
					CurrentKeys.CopyTo(PreviousKeys, 0);
					var keysBuffer = SDL.SDL_GetKeyboardState(out int keysBufferLength);
					System.Runtime.InteropServices.Marshal.Copy(keysBuffer, CurrentKeys, 0, keysBufferLength);

					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE))
						return false;

					var player = simulation.GetPlayerEntity();

					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_UP))
						player.HandleButton(InputButton.Up, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_UP))
						player.HandleButton(InputButton.Up, InputButtonState.Released);

					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_LEFT))
						player.HandleButton(InputButton.Left, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_LEFT))
						player.HandleButton(InputButton.Left, InputButtonState.Released);

					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_DOWN))
						player.HandleButton(InputButton.Down, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_DOWN))
						player.HandleButton(InputButton.Down, InputButtonState.Released);

					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_RIGHT))
						player.HandleButton(InputButton.Right, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_RIGHT))
						player.HandleButton(InputButton.Right, InputButtonState.Released);

					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_LSHIFT))
						player.HandleButton(InputButton.R2, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_LSHIFT))
						player.HandleButton(InputButton.R2, InputButtonState.Released);
				}
			}
	
			return true;
		}

		private static bool KeyPressed(SDL.SDL_Scancode scanCode)
		{
			return CurrentKeys[(int)scanCode] == 1 && PreviousKeys[(int)scanCode] == 0;
		}

		private static bool KeyReleased(SDL.SDL_Scancode scanCode)
		{
			return CurrentKeys[(int)scanCode] == 0 && PreviousKeys[(int)scanCode] == 1;
		}
	}
}
