using System;
using Ujeby.Plosinofka.Engine.Core;

namespace Ujeby.Plosinofka.Engine.SDL
{
	public class SDL2Input : Input
	{
		public Action<KeyboardButton> KeyboardButtonHandler { get; set; }

		private readonly byte[] CurrentKeys = new byte[(int)SDL2.SDL.SDL_Scancode.SDL_NUM_SCANCODES];
		private readonly byte[] PreviousKeys = new byte[(int)SDL2.SDL.SDL_Scancode.SDL_NUM_SCANCODES];

		public override bool Handle(Engine.Core.Game simulation)
		{
			while (SDL2.SDL.SDL_PollEvent(out SDL2.SDL.SDL_Event e) != 0)
			{
				switch (e.type)
				{
					case SDL2.SDL.SDL_EventType.SDL_QUIT:
						return false;
				};

				if (e.type == SDL2.SDL.SDL_EventType.SDL_KEYUP || e.type == SDL2.SDL.SDL_EventType.SDL_KEYDOWN)
				{
					CurrentKeys.CopyTo(PreviousKeys, 0);
					var keysBuffer = SDL2.SDL.SDL_GetKeyboardState(out int keysBufferLength);
					System.Runtime.InteropServices.Marshal.Copy(keysBuffer, CurrentKeys, 0, keysBufferLength);

					if (KeyPressed(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE))
						return false;

					var player = simulation.Player;
					if (player != null)
					{
						// TODO handle LS/RS as direction vector + stick type

						// LS
						if (KeyPressed(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_UP))
							player.HandleButton(InputButton.Up, InputButtonState.Pressed);
						if (KeyReleased(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_UP))
							player.HandleButton(InputButton.Up, InputButtonState.Released);

						if (KeyPressed(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_DOWN))
							player.HandleButton(InputButton.Down, InputButtonState.Pressed);
						if (KeyReleased(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_DOWN))
							player.HandleButton(InputButton.Down, InputButtonState.Released);

						if (KeyPressed(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_LEFT))
							player.HandleButton(InputButton.Left, InputButtonState.Pressed);
						if (KeyReleased(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_LEFT))
							player.HandleButton(InputButton.Left, InputButtonState.Released);

						if (KeyPressed(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_RIGHT))
							player.HandleButton(InputButton.Right, InputButtonState.Pressed);
						if (KeyReleased(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_RIGHT))
							player.HandleButton(InputButton.Right, InputButtonState.Released);

						// X
						if (KeyPressed(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_A))
							player.HandleButton(InputButton.X, InputButtonState.Pressed);
						if (KeyReleased(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_A))
							player.HandleButton(InputButton.X, InputButtonState.Released);

						// Y
						if (KeyPressed(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_W))
							player.HandleButton(InputButton.Y, InputButtonState.Pressed);
						if (KeyReleased(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_W))
							player.HandleButton(InputButton.Y, InputButtonState.Released);

						// A
						if (KeyPressed(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_S))
							player.HandleButton(InputButton.A, InputButtonState.Pressed);
						if (KeyReleased(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_S))
							player.HandleButton(InputButton.A, InputButtonState.Released);

						// B
						if (KeyPressed(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_D))
							player.HandleButton(InputButton.B, InputButtonState.Pressed);
						if (KeyReleased(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_D))
							player.HandleButton(InputButton.B, InputButtonState.Released);

						// LB
						if (KeyPressed(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_Q))
							player.HandleButton(InputButton.LB, InputButtonState.Pressed);
						if (KeyReleased(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_Q))
							player.HandleButton(InputButton.LB, InputButtonState.Released);

						// LT
						if (KeyPressed(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_LSHIFT))
							player.HandleButton(InputButton.LT, InputButtonState.Pressed);
						if (KeyReleased(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_LSHIFT))
							player.HandleButton(InputButton.LT, InputButtonState.Released);

						// RB
						if (KeyPressed(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_E))
							player.HandleButton(InputButton.RB, InputButtonState.Pressed);
						if (KeyReleased(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_E))
							player.HandleButton(InputButton.RB, InputButtonState.Released);

						// RT
						if (KeyPressed(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_RSHIFT))
							player.HandleButton(InputButton.RT, InputButtonState.Pressed);
						if (KeyReleased(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_RSHIFT))
							player.HandleButton(InputButton.RT, InputButtonState.Released);

						// Back
						if (KeyPressed(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_B))
							player.HandleButton(InputButton.Back, InputButtonState.Pressed);
						if (KeyReleased(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_B))
							player.HandleButton(InputButton.Back, InputButtonState.Released);

						// Start
						if (KeyPressed(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_M))
							player.HandleButton(InputButton.Start, InputButtonState.Pressed);
						if (KeyReleased(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_M))
							player.HandleButton(InputButton.Start, InputButtonState.Released);
					}

					// F1 - F12 toggles
					for (var i = SDL2.SDL.SDL_Scancode.SDL_SCANCODE_F1; i < SDL2.SDL.SDL_Scancode.SDL_SCANCODE_F12; i++)
					{
						if (!KeyPressed(i))
							continue;

						KeyboardButtonHandler((KeyboardButton)(i - SDL2.SDL.SDL_Scancode.SDL_SCANCODE_F1));
					}
				}
			}
	
			return true;
		}

		private bool KeyPressed(SDL2.SDL.SDL_Scancode scanCode)
		{
			return CurrentKeys[(int)scanCode] == 1 && PreviousKeys[(int)scanCode] == 0;
		}

		private bool KeyReleased(SDL2.SDL.SDL_Scancode scanCode)
		{
			return CurrentKeys[(int)scanCode] == 0 && PreviousKeys[(int)scanCode] == 1;
		}
	}
}
