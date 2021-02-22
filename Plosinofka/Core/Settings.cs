using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;

namespace Ujeby.Plosinofka.Game
{
	internal enum VisualSetting : int
	{
		PerPixelShading = 0,

		Count,
	}

	internal enum DebugSetting : int
	{
		DrawOSD = 0,
		DrawFrameTimers,
		DrawHistory,
		DrawAABB,
		DrawCamera,
		DrawIds,

		Count,
	}

	internal class Settings : Singleton<Settings>
	{
		public Settings()
		{
			Load();
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
		internal class InputMapping
		{
			public InputButton Jump = InputButton.Up;
			public InputButton Crouch = InputButton.Down;
			public InputButton Run = InputButton.LT;
			public InputButton Dash = InputButton.RB;
			public InputButton MeleeAttack = InputButton.X;
			public InputButton Menu = InputButton.Start;

			public KeyboardButton[] VisualSettings = new KeyboardButton[(int)VisualSetting.Count]
			{
				KeyboardButton.F1,
			};

			public KeyboardButton[] DebugSettings = new KeyboardButton[(int)DebugSetting.Count]
			{
				KeyboardButton.F5,
				KeyboardButton.F6,
				KeyboardButton.F7,
				KeyboardButton.F8,
				KeyboardButton.F9,
				KeyboardButton.F10,
			};
		}

		public InputMapping InputMappings { get; private set; } = new InputMapping();

		public bool[] VisualToggles { get; private set; } = new bool[(int)VisualSetting.Count];
		public bool[] DebugToggles { get; private set; } = new bool[(int)DebugSetting.Count];

		public void ToggleVisual(VisualSetting toggle)
		{
			VisualToggles[(int)toggle] = !VisualToggles[(int)toggle];

			Log.Add($"Renderer.SettingsChanged({ toggle }): { GetVisual(toggle) }");
			SettingsChanged();
		}

		public void ToggleDebug(DebugSetting toggle)
		{
			DebugToggles[(int)toggle] = !DebugToggles[(int)toggle];

			Log.Add($"Renderer.SettingsChanged({ toggle }): { GetDebug(toggle) }");
			SettingsChanged();
		}

		internal bool GetVisual(VisualSetting setting)
		{
			return VisualToggles[(int)setting];
		}

		internal bool GetDebug(DebugSetting setting)
		{
			return DebugToggles[(int)setting];
		}

		private void Load()
		{
			// TODO read settings from file
		}

		private void Save()
		{
			// TODO save settings to file
		}

		private void SettingsChanged()
		{
			Save();
		}
	}
}
