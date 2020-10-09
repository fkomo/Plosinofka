using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka
{
	internal class Settings
	{
		private static Settings Instance;
		public static Settings Current
		{
			get
			{
				if (Instance == null)
					Instance = new Settings();

				return Instance;
			}
		}

		internal class Controls
		{
			public InputButton Running = InputButton.R2;
			public InputButton Jump = InputButton.Up;
			public InputButton Crouch = InputButton.Down;
		}
		public Controls PlayerControls { get; private set; } = new Controls();
	}
}
