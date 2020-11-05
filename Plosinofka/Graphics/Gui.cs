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

		public void Render(AABB view, double interpolation)
		{
			// gui text
			Renderer.Instance.RenderText(view, new Vector2i(0, 0),
				"01234567890 ABCDEFGHIJKLMONOPRSTUVWXYZ abcdefghijklmnopqrstuvwxyz +-*= []{}<>\\/'\".:,;?|_");
		}
	}
}
