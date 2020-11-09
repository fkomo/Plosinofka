using System.Collections.Generic;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Graphics;

namespace Ujeby.Plosinofka.Game.Graphics
{
	public class Gui : IRender
	{
		public static Gui Instance { get; set; } = new Gui();

		public Gui()
		{
		}

		public double LastGuiDuration { get; protected set; }

		public void Render(AABB view, double interpolation)
		{
			var start = Engine.Core.Game.GetElapsed();

			//Renderer.Instance.RenderText(view, new Vector2i(0, 0),
			//	"0123456789 ABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyz +-*= ~@#$%^& ()[]{}<> \\/'\".:,;?|_",
			//	Color4b.White,
			//	0.5);

			var lines = new List<string>()
			{
				$"fps: { Engine.Core.Game.Fps }",
				$"update: { Simulation.Instance.LastUpdateDuration:0.00}ms",
				$"render: { Renderer.Instance.LastFrameDuration:0.00}ms",
				$" + shading: { Renderer.Instance.LastShadingDuration:0.00}ms",
				$" + gui: { LastGuiDuration:0.00}ms",
				"",
			};

			for (var i = 0; i < Settings.Current.InputMappings.VisualSettings.Length; i++)
			{
				var settingName = ((VisualSetting)i).ToString();
				var key = Settings.Current.InputMappings.VisualSettings[i].ToString();

				var state = Settings.Current.GetVisual((VisualSetting)i) ? "enabled" : "disabled";
				lines.Add($"[{ key.ToUpper() }] { settingName }: { state }");
			}
			lines.Add("");

			for (var i = 0; i < Settings.Current.InputMappings.DebugSettings.Length; i++)
			{
				var settingName = ((DebugSetting)i).ToString();
				var key = Settings.Current.InputMappings.DebugSettings[i].ToString();

				var state = Settings.Current.GetDebug((DebugSetting)i) ? "enabled" : "disabled";
				lines.Add($"[{ key.ToUpper() }] { settingName }: { state }");
			}

			Renderer.Instance.RenderTextLines(view, new Vector2i(5, 5), lines.ToArray(), Color4b.White, 0.5);

			LastGuiDuration = Engine.Core.Game.GetElapsed() - start;
		}
	}
}
