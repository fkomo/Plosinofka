using System.Collections.Generic;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Entities;
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

			var buffer = new List<TextLine>()
			{
				new Text { Value = $"fps: { Engine.Core.Game.Fps }", Color = Color4b.White },
				new Text { Value = $"update: { Simulation.Instance.LastUpdateDuration:0.00}ms", Color = Color4b.White },
				new Text { Value = $"render: { Renderer.Instance.LastFrameDuration:0.00}ms", Color = Color4b.White },
				new Text { Value = $"+ shading: { Renderer.Instance.LastShadingDuration:0.00}ms", Color = Color4b.White },
				new Text { Value = $"+ gui: { LastGuiDuration:0.00}ms", Color = Color4b.White },
			};

			buffer.Add(new EmptyLine());
			buffer.Add(new Text { Value = Simulation.Instance.Player.ToString(), Color = Color4b.White });

			buffer.Add(new EmptyLine());
			buffer.Add(new Text { Value = Simulation.Instance.Camera.ToString(), Color = Color4b.White });

			buffer.Add(new EmptyLine());
			buffer.Add(new Text { Value = $"{ nameof(VisualSetting) }s", Color = Color4b.White });
			for (var i = 0; i < Settings.Instance.InputMappings.VisualSettings.Length; i++)
			{
				var settingName = ((VisualSetting)i).ToString();
				var key = Settings.Instance.InputMappings.VisualSettings[i].ToString();

				var state = Settings.Instance.GetVisual((VisualSetting)i) ? "enabled" : "disabled";
				buffer.Add(new Text { Value = $"+ { settingName } [{ key.ToUpper() }]: { state }",
					Color = Settings.Instance.GetVisual((VisualSetting)i) ? Color4b.White : Color4b.Gray });
			}

			buffer.Add(new EmptyLine());
			buffer.Add(new Text { Value = $"{ nameof(DebugSetting) }s", Color = Color4b.White });
			for (var i = 0; i < Settings.Instance.InputMappings.DebugSettings.Length; i++)
			{
				var settingName = ((DebugSetting)i).ToString();
				var key = Settings.Instance.InputMappings.DebugSettings[i].ToString();

				var state = Settings.Instance.GetDebug((DebugSetting)i) ? "enabled" : "disabled";
				buffer.Add(new Text { Value = $"+ { settingName } [{ key.ToUpper() }]: { state }", 
					Color = Settings.Instance.GetDebug((DebugSetting)i) ? Color4b.White : Color4b.Gray });
			}

			var lines = buffer.ToArray();

			// TODO draw gui elements
			//var textSize = Renderer.Instance.GetTextSize(lines);
			//Renderer.Instance.RenderRectangleOverlay(view, new AABB(Vector2f.Zero, textSize), 
			//	Color4b.Red);

			Renderer.Instance.RenderTextLinesOverlay(view, new Vector2i(5, 5), lines, Color4b.White, 0.5);

			LastGuiDuration = Engine.Core.Game.GetElapsed() - start;
		}
	}
}
