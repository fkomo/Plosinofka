using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Core
{
	public enum VisualSetting
	{
		Shading,

		Count,
	}

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

		private Settings()
		{
			// TODO read saved settings from file
		}

		/// <summary>
		/// LS - moving
		/// RS - range attack in specified direction (single shot / auto)
		/// LB - 
		/// RB - dash (while jumping/walking)
		/// LT - running
		/// RT -  
		/// A - jump/double jump (jump + down = dive)
		/// B - 
		/// X - melee attack
		/// Y - 
		/// </summary>
		internal class Controls
		{
			public InputButton Jump = InputButton.Up;
			public InputButton Crouch = InputButton.Down;

			public InputButton Run = InputButton.LT;
			public InputButton Dash = InputButton.RB;

			public InputButton MeleeAttack = InputButton.X;

			public InputButton Menu = InputButton.Start;
		}
		public Controls PlayerControls { get; private set; } = new Controls();

		public bool[] VisualToggles { get; private set; } = new bool[(int)VisualSetting.Count];

		public void ToggleVisual(VisualSetting toggle)
		{
			VisualToggles[(int)toggle] = !VisualToggles[(int)toggle];
			Renderer.Instance.SettingsChanged(toggle);
		}

		internal bool GetVisual(VisualSetting setting)
		{
			return VisualToggles[(int)setting];
		}
	}
}
