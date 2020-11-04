using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Core
{
	public enum VisualSetting
	{
		PerPixelShading,

		Count,
	}

	public enum DebugSetting
	{
		MovementHistory,
		DrawAABB,
		DrawVectors,

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
