using SDL2;
using Ujeby.Plosinofka.Engine.Core;

namespace Ujeby.Plosinofka.Game
{
	public class SDL2Input : Input
	{
		private readonly byte[] CurrentKeys = new byte[(int)SDL.SDL_Scancode.SDL_NUM_SCANCODES];
		private readonly byte[] PreviousKeys = new byte[(int)SDL.SDL_Scancode.SDL_NUM_SCANCODES];

		public override bool Handle(Simulation simulation)
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

					var player = simulation.Player;

					// TODO handle LS/RS as direction vector + stick type

					// LS
					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_UP))
						player.HandleButton(InputButton.Up, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_UP))
						player.HandleButton(InputButton.Up, InputButtonState.Released);

					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_DOWN))
						player.HandleButton(InputButton.Down, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_DOWN))
						player.HandleButton(InputButton.Down, InputButtonState.Released);

					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_LEFT))
						player.HandleButton(InputButton.Left, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_LEFT))
						player.HandleButton(InputButton.Left, InputButtonState.Released);

					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_RIGHT))
						player.HandleButton(InputButton.Right, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_RIGHT))
						player.HandleButton(InputButton.Right, InputButtonState.Released);

					// X
					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_A))
						player.HandleButton(InputButton.X, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_A))
						player.HandleButton(InputButton.X, InputButtonState.Released);

					// Y
					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_W))
						player.HandleButton(InputButton.Y, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_W))
						player.HandleButton(InputButton.Y, InputButtonState.Released);

					// A
					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_S))
						player.HandleButton(InputButton.A, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_S))
						player.HandleButton(InputButton.A, InputButtonState.Released);

					// B
					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_D))
						player.HandleButton(InputButton.B, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_D))
						player.HandleButton(InputButton.B, InputButtonState.Released);

					// LB
					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_Q))
						player.HandleButton(InputButton.LB, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_Q))
						player.HandleButton(InputButton.LB, InputButtonState.Released);
					
					// LT
					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_LSHIFT))
						player.HandleButton(InputButton.LT, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_LSHIFT))
						player.HandleButton(InputButton.LT, InputButtonState.Released);

					// RB
					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_E))
						player.HandleButton(InputButton.RB, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_E))
						player.HandleButton(InputButton.RB, InputButtonState.Released);

					// RT
					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_RSHIFT))
						player.HandleButton(InputButton.RT, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_RSHIFT))
						player.HandleButton(InputButton.RT, InputButtonState.Released);

					// Back
					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_B))
						player.HandleButton(InputButton.Back, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_B))
						player.HandleButton(InputButton.Back, InputButtonState.Released);

					// Start
					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_M))
						player.HandleButton(InputButton.Start, InputButtonState.Pressed);
					if (KeyReleased(SDL.SDL_Scancode.SDL_SCANCODE_M))
						player.HandleButton(InputButton.Start, InputButtonState.Released);

					// visual settings
					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_F1))
						Settings.Current.ToggleVisual(VisualSetting.PerPixelShading);

					// debug settings
					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_F5))
						Settings.Current.ToggleDebug(DebugSetting.DrawHistory);
					if (KeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_F6))
						Settings.Current.ToggleDebug(DebugSetting.DrawAABB);
				}
			}
	
			return true;
		}

		private bool KeyPressed(SDL.SDL_Scancode scanCode)
		{
			return CurrentKeys[(int)scanCode] == 1 && PreviousKeys[(int)scanCode] == 0;
		}

		private bool KeyReleased(SDL.SDL_Scancode scanCode)
		{
			return CurrentKeys[(int)scanCode] == 0 && PreviousKeys[(int)scanCode] == 1;
		}
	}
}
