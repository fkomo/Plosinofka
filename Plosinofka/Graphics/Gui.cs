using System.Collections.Generic;
using Ujeby.Plosinofka.Engine.Common;
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
			var start = Engine.Core.GameLoop.GetElapsed();

			//Renderer.Instance.RenderText(view, new Vector2i(0, 0),
			//	"0123456789 ABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyz +-*= ~@#$%^& ()[]{}<> \\/'\".:,;?|_",
			//	Color4b.White,
			//	0.5);

			if (Settings.Instance.GetDebug(DebugSetting.DrawFrameTimers))
			{
				var white = new Color4b(Color4b.White) { A = 0x7f };
				var red = new Color4b(Color4b.Red) { A = 0x7f };
				var green = new Color4b(Color4b.Green) { A = 0x7f };
				var blue = new Color4b(Color4b.Blue) { A = 0x7f };

				var render = Renderer.Instance.LastFrameDuration;
				Renderer.Instance.RenderRectangleOverlay(view, new AABB(new Vector2f(0, 0), new Vector2f(2, render)), red);

				var update = Engine.Core.Game.Instance.LastUpdateDuration;
				Renderer.Instance.RenderRectangleOverlay(view, new AABB(new Vector2f(0, render), new Vector2f(2, render + update)), green);

				//Renderer.Instance.RenderRectangleOverlay(view, new AABB(new Vector2f(0, 0), new Vector2f(2, 32)), blue);

				Renderer.Instance.RenderLineOverlay(view, new Vector2i(0, 16), new Vector2i((int)view.Size.X, 16), white);
			}

			if (Settings.Instance.GetDebug(DebugSetting.DrawOSD))
			{
				var buffer = new List<TextLine>()
				{
					new Text { Value = $"fps: { Engine.Core.GameLoop.Fps }", Color = Color4b.White },
					//new Text { Value = $"update: { Engine.Core.Game.Instance.LastUpdateDuration:0.00}ms", Color = Color4b.White },
					//new Text { Value = $"render: { Renderer.Instance.LastFrameDuration:0.00}ms", Color = Color4b.White },
					//new Text { Value = $"+ shading: { Renderer.Instance.LastShadingDuration:0.00}ms", Color = Color4b.White },
					//new Text { Value = $"+ gui: { LastGuiDuration:0.00}ms", Color = Color4b.White },
				};

				buffer.Add(new EmptyLine());
				buffer.Add(new Text { Value = Engine.Core.Game.Instance.Player.ToString(), Color = Color4b.White });

				buffer.Add(new EmptyLine());
				buffer.Add(new Text { Value = Engine.Core.Game.Instance.Camera.ToString(), Color = Color4b.White });

				buffer.Add(new EmptyLine());
				buffer.Add(new Text { Value = $"{ nameof(VisualSetting) }s", Color = Color4b.White });
				for (var i = 0; i < Settings.Instance.InputMappings.VisualSettings.Length; i++)
				{
					var settingName = ((VisualSetting)i).ToString();
					var key = Settings.Instance.InputMappings.VisualSettings[i].ToString();

					var state = Settings.Instance.GetVisual((VisualSetting)i) ? "enabled" : "disabled";
					buffer.Add(new Text
					{
						Value = $"+ { settingName } [{ key.ToUpper() }]: { state }",
						Color = Settings.Instance.GetVisual((VisualSetting)i) ? Color4b.White : Color4b.Gray
					});
				}

				buffer.Add(new EmptyLine());
				buffer.Add(new Text { Value = $"{ nameof(DebugSetting) }s", Color = Color4b.White });
				for (var i = 0; i < Settings.Instance.InputMappings.DebugSettings.Length; i++)
				{
					var settingName = ((DebugSetting)i).ToString();
					var key = Settings.Instance.InputMappings.DebugSettings[i].ToString();

					var state = Settings.Instance.GetDebug((DebugSetting)i) ? "enabled" : "disabled";
					buffer.Add(new Text
					{
						Value = $"+ { settingName } [{ key.ToUpper() }]: { state }",
						Color = Settings.Instance.GetDebug((DebugSetting)i) ? Color4b.White : Color4b.Gray
					});
				}

				var lines = buffer.ToArray();

				// TODO draw gui elements
				//var textSize = Renderer.Instance.GetTextSize(lines);
				//Renderer.Instance.RenderRectangleOverlay(view, new AABB(Vector2f.Zero, textSize), 
				//	Color4b.Red);

				Renderer.Instance.RenderTextLinesOverlay(view, new Vector2i(5, 5), lines, Color4b.White, 0.5);
			}

			LastGuiDuration = Engine.Core.GameLoop.GetElapsed() - start;
		}
	}
}
